using Application.AdminImport;

using Domain.Entities.Deck;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Shared.Contracts.AdminImport;

namespace Infrastructure.AdminImport;

public sealed class QuestionImportService(AppDbContext db) : IQuestionImportService
{
    public async Task<AdminImportQuestionsResult> ImportAsync(
        IReadOnlyList<AdminImportQuestionItem> items,
        IReadOnlyDictionary<int, int>? csvLineByIndex,
        CancellationToken ct)
    {
        if (items.Count == 0)
        {
            return new AdminImportQuestionsResult(0, 0, 0, 0, Array.Empty<ImportError>());
        }

        var errors = new List<ImportError>();
        var normalizedItems = new List<NormalizedItem>();
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int skipped = 0;

        for (int i = 0; i < items.Count; i++)
        {
            AdminImportQuestionItem item = items[i];
            if (!TryNormalize(item, out NormalizedItem normalized, out IReadOnlyList<ImportIssue> issues))
            {
                foreach (ImportIssue issue in issues)
                {
                    errors.Add(CreateError(i, csvLineByIndex, issue.Field, issue.Message));
                }

                skipped++;
                continue;
            }

            string key = BuildKey(normalized.TestVersion, normalized.QuestionNo);
            if (!seenKeys.Add(key))
            {
                errors.Add(CreateError(i, csvLineByIndex, "naturalKey", "Duplicate (TestVersion, QuestionNo) in batch."));
                skipped++;
                continue;
            }

            normalizedItems.Add(normalized);
        }

        if (normalizedItems.Count == 0)
        {
            return new AdminImportQuestionsResult(0, 0, skipped, errors.Count, errors);
        }

        var testVersions = normalizedItems
            .Select(x => x.TestVersion)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var codeByVersion = testVersions.ToDictionary(
            tv => tv,
            tv => $"civics-{tv}",
            StringComparer.OrdinalIgnoreCase);

        var versionByCode = codeByVersion
            .ToDictionary(kv => kv.Value, kv => kv.Key, StringComparer.OrdinalIgnoreCase);

        List<Deck> existingDecks = await db.Decks
            .Where(d => codeByVersion.Values.Contains(d.Code))
            .ToListAsync(ct);

        var deckByVersion = new Dictionary<string, Deck>(StringComparer.OrdinalIgnoreCase);
        foreach (Deck deck in existingDecks)
        {
            if (versionByCode.TryGetValue(deck.Code, out string? tv))
            {
                deckByVersion[tv] = deck;
            }
        }

        foreach (string testVersion in testVersions)
        {
            if (deckByVersion.ContainsKey(testVersion))
            {
                continue;
            }

            var deck = new Deck
            {
                Code = codeByVersion[testVersion],
                Name = $"US Civics ({testVersion})",
                IsActive = true
            };

            db.Decks.Add(deck);
            deckByVersion[testVersion] = deck;
        }

        List<Question> existingQuestions = await db.Questions
            .Include(q => q.Options)
            .Where(q => testVersions.Contains(q.TestVersion))
            .ToListAsync(ct);

        var existingByKey = new Dictionary<string, Question>(StringComparer.OrdinalIgnoreCase);
        foreach (Question question in existingQuestions)
        {
            string key = BuildKey(question.TestVersion, question.QuestionNo);
            existingByKey[key] = question;
        }

        int created = 0;
        int updated = 0;

        foreach (NormalizedItem item in normalizedItems)
        {
            string key = BuildKey(item.TestVersion, item.QuestionNo);
            Deck deck = deckByVersion[item.TestVersion];

            if (!existingByKey.TryGetValue(key, out Question? question))
            {
                var newQuestion = new Question
                {
                    Deck = deck,
                    TestVersion = item.TestVersion,
                    QuestionNo = item.QuestionNo,
                    Type = item.Type,
                    PromptEn = item.PromptEn,
                    PromptVi = item.PromptVi,
                    PromptViPhonetic = item.PromptViPhonetic,
                    ExplainEn = item.ExplainEn,
                    ExplainVi = item.ExplainVi,
                    CorrectOptionKey = item.CorrectOptionKey,
                    Options = item.Options
                        .Select(o => new QuestionOption
                        {
                            Key = o.Key,
                            TextEn = o.TextEn,
                            TextVi = o.TextVi,
                            SortOrder = o.SortOrder
                        })
                        .ToList()
                };

                db.Questions.Add(newQuestion);
                created++;
                continue;
            }

            question.DeckId = deck.DeckId;
            question.TestVersion = item.TestVersion;
            question.QuestionNo = item.QuestionNo;
            question.Type = item.Type;
            question.PromptEn = item.PromptEn;
            question.PromptVi = item.PromptVi;
            question.PromptViPhonetic = item.PromptViPhonetic;
            question.ExplainEn = item.ExplainEn;
            question.ExplainVi = item.ExplainVi;
            question.CorrectOptionKey = item.CorrectOptionKey;

            var incomingByKey = item.Options.ToDictionary(o => o.Key, StringComparer.OrdinalIgnoreCase);
            var existingOptionsByKey = question.Options
                .ToDictionary(o => o.Key, StringComparer.OrdinalIgnoreCase);

            foreach (AdminImportQuestionOptionItem option in item.Options)
            {
                if (existingOptionsByKey.TryGetValue(option.Key, out QuestionOption? existingOption))
                {
                    existingOption.TextEn = option.TextEn;
                    existingOption.TextVi = option.TextVi;
                    existingOption.SortOrder = option.SortOrder;
                    continue;
                }

                question.Options.Add(new QuestionOption
                {
                    QuestionId = question.QuestionId,
                    Key = option.Key,
                    TextEn = option.TextEn,
                    TextVi = option.TextVi,
                    SortOrder = option.SortOrder
                });
            }

            var toRemove = question.Options
                .Where(o => !incomingByKey.ContainsKey(o.Key))
                .ToList();

            if (toRemove.Count > 0)
            {
                db.QuestionOptions.RemoveRange(toRemove);
            }

            updated++;
        }

        await db.SaveChangesAsync(ct);

        return new AdminImportQuestionsResult(
            created,
            updated,
            skipped,
            errors.Count,
            errors);
    }

    private static ImportError CreateError(
        int index,
        IReadOnlyDictionary<int, int>? csvLineByIndex,
        string? field,
        string message)
    {
        int? csvLine = null;
        if (csvLineByIndex is not null && csvLineByIndex.TryGetValue(index, out int line))
        {
            csvLine = line;
        }

        return new ImportError(index, csvLine, field, message);
    }

    private static bool TryNormalize(
        AdminImportQuestionItem item,
        out NormalizedItem normalized,
        out IReadOnlyList<ImportIssue> issues)
    {
        var issueList = new List<ImportIssue>();

        string? testVersion = Clean(item.TestVersion);
        if (string.IsNullOrWhiteSpace(testVersion))
        {
            issueList.Add(new ImportIssue("testVersion", "TestVersion is required."));
        }

        if (item.QuestionNo <= 0)
        {
            issueList.Add(new ImportIssue("questionNo", "QuestionNo must be greater than 0."));
        }

        string? type = Clean(item.Type);
        if (string.IsNullOrWhiteSpace(type))
        {
            issueList.Add(new ImportIssue("type", "Type is required."));
        }

        string? promptEn = Clean(item.PromptEn);
        if (string.IsNullOrWhiteSpace(promptEn))
        {
            issueList.Add(new ImportIssue("promptEn", "PromptEn is required."));
        }

        string? correctKey = Clean(item.CorrectOptionKey);
        if (string.IsNullOrWhiteSpace(correctKey))
        {
            issueList.Add(new ImportIssue("correctOptionKey", "CorrectOptionKey is required."));
        }

        var options = new List<AdminImportQuestionOptionItem>();
        var optionKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int optionIndex = 0;

        foreach (AdminImportQuestionOptionItem option in item.Options ?? Array.Empty<AdminImportQuestionOptionItem>())
        {
            optionIndex++;
            string? key = Clean(option.Key);
            string? textEn = Clean(option.TextEn);
            string? textVi = Clean(option.TextVi);

            if (string.IsNullOrWhiteSpace(key))
            {
                issueList.Add(new ImportIssue("options", "Option key is required."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(textEn))
            {
                issueList.Add(new ImportIssue("options", $"Option '{key}' TextEn is required."));
                continue;
            }

            if (!optionKeys.Add(key))
            {
                issueList.Add(new ImportIssue("options", $"Duplicate option key '{key}'."));
                continue;
            }

            int sortOrder = option.SortOrder > 0 ? option.SortOrder : optionIndex;
            options.Add(new AdminImportQuestionOptionItem(
                key.ToUpperInvariant(),
                textEn,
                textVi,
                sortOrder));
        }

        if (options.Count < 2)
        {
            issueList.Add(new ImportIssue("options", "At least 2 options are required."));
        }

        if (!string.IsNullOrWhiteSpace(correctKey) &&
            !options.Any(o => string.Equals(o.Key, correctKey, StringComparison.OrdinalIgnoreCase)))
        {
            issueList.Add(new ImportIssue("correctOptionKey", "CorrectOptionKey must match an option key."));
        }

        if (issueList.Count > 0)
        {
            issues = issueList;
            normalized = default!;
            return false;
        }

        normalized = new NormalizedItem(
            testVersion!,
            item.QuestionNo,
            type!,
            promptEn!,
            Clean(item.PromptVi),
            Clean(item.PromptViPhonetic),
            Clean(item.ExplainEn),
            Clean(item.ExplainVi),
            correctKey!.ToUpperInvariant(),
            options);

        issues = Array.Empty<ImportIssue>();
        return true;
    }

    private static string BuildKey(string testVersion, int questionNo)
        => $"{testVersion.Trim()}|{questionNo}";

    private static string? Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private sealed record NormalizedItem(
        string TestVersion,
        int QuestionNo,
        string Type,
        string PromptEn,
        string? PromptVi,
        string? PromptViPhonetic,
        string? ExplainEn,
        string? ExplainVi,
        string CorrectOptionKey,
        IReadOnlyList<AdminImportQuestionOptionItem> Options
    );

    private sealed record ImportIssue(string Field, string Message);
}
