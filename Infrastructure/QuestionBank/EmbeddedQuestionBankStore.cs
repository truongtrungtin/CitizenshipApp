using System.Reflection;
using System.Text.Json;

using Shared.Contracts.Deck;

namespace Infrastructure.QuestionBank;

/// <summary>
///     Loads a read-only question bank from an embedded JSON resource.
///     Why embedded resource (MVP):
///     - Zero runtime file deployment concerns (no "copy to output" mistakes).
///     - Works the same in local dev and in container hosting.
///     - Prevents accidental overwrites since the bank is immutable at runtime.
///     Later (EPIC 5+):
///     - Replace this with DB-backed store or admin import pipeline.
/// </summary>
internal sealed class EmbeddedQuestionBankStore : IQuestionBankStore
{
    // Resource name = <DefaultNamespace>.<Folder>.<FileName>
    // Infrastructure project's default namespace is "Infrastructure".
    private const string ResourceName = "Infrastructure.QuestionBank.question-bank.sample.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        // Why: We want strict JSON without trailing commas in production.
        // Set to true only if you prefer lenient parsing during content iteration.
        AllowTrailingCommas = false
    };

    // Why: We want to parse the JSON only once for the whole app lifetime.
    // A singleton store holds a cached snapshot.
    private readonly object _gate = new();
    private Task<QuestionBankSnapshot>? _cached;

    public Task<QuestionBankSnapshot> GetSnapshotAsync(CancellationToken ct)
    {
        // We intentionally ignore the provided CancellationToken for the initial load.
        // Why:
        // - If the first request is cancelled, we do not want to keep a cancelled task cached.
        // - The question bank is tiny (for MVP) and loading is fast.
        lock (_gate)
        {
            _cached ??= LoadAsync(CancellationToken.None);
            return _cached;
        }
    }

    private static async Task<QuestionBankSnapshot> LoadAsync(CancellationToken ct)
    {
        Assembly assembly = typeof(EmbeddedQuestionBankStore).Assembly;

        await using Stream stream = assembly.GetManifestResourceStream(ResourceName)
                                    ?? throw new InvalidOperationException(
                                        $"Question bank resource '{ResourceName}' was not found. " +
                                        "Ensure Infrastructure.csproj embeds the JSON file.");

        QuestionBankFile file = await JsonSerializer.DeserializeAsync<QuestionBankFile>(stream, JsonOptions, ct)
                                ?? throw new InvalidOperationException("Question bank JSON is empty or invalid.");

        // ---------------------------
        // Validation (fail fast)
        // ---------------------------

        if (file.Decks.Count == 0)
        {
            throw new InvalidOperationException("Question bank must contain at least one deck.");
        }

        // Ensure unique deck ids
        if (file.Decks.Select(d => d.Id).Distinct().Count() != file.Decks.Count)
        {
            throw new InvalidOperationException("Duplicate Deck Id detected in question bank JSON.");
        }

        // Ensure unique question ids
        if (file.Questions.Select(q => q.Id).Distinct().Count() != file.Questions.Count)
        {
            throw new InvalidOperationException("Duplicate Question Id detected in question bank JSON.");
        }

        // Ensure every question's DeckId exists
        var deckIdSet = file.Decks.Select(d => d.Id).ToHashSet();
        var unknownDeckQuestions = file.Questions.Where(q => !deckIdSet.Contains(q.DeckId)).ToList();
        if (unknownDeckQuestions.Count > 0)
        {
            throw new InvalidOperationException(
                "Question bank contains questions that reference unknown DeckId. " +
                $"First offending QuestionId: {unknownDeckQuestions[0].Id}");
        }

        // Group questions per deck
        var questionsByDeck = file.Questions
            .GroupBy(q => q.DeckId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<Question>)g.ToList());

        // Build id lookup
        var questionsById = file.Questions.ToDictionary(q => q.Id, q => q);

        return new QuestionBankSnapshot
        {
            Decks = file.Decks,
            QuestionsByDeckId = questionsByDeck,
            QuestionsById = questionsById
        };
    }
}
