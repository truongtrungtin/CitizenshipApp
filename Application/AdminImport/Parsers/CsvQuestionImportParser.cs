using System.Globalization;

using CsvHelper;
using CsvHelper.Configuration;

using Shared.Contracts.AdminImport;

namespace Application.AdminImport.Parsers;

public sealed class CsvQuestionImportParser
{
    public async Task<CsvParseResult> ParseAsync(Stream stream, CancellationToken ct)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim,
            BadDataFound = null,
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, config);

        var items = new List<AdminImportQuestionItem>();
        var lineByIndex = new Dictionary<int, int>();

        await csv.ReadAsync();
        csv.ReadHeader();

        while (await csv.ReadAsync())
        {
            ct.ThrowIfCancellationRequested();

            int line = csv.Context?.Parser?.Row ?? 0;

            string? GetField(string name)
                => csv.TryGetField(name, out string? value) ? value : null;

            string testVersion = Clean(GetField("TestVersion")) ?? string.Empty;
            int questionNo = int.TryParse(Clean(GetField("QuestionNo")), out int parsedNo) ? parsedNo : 0;
            string type = Clean(GetField("Type")) ?? string.Empty;
            string promptEn = Clean(GetField("PromptEn")) ?? string.Empty;
            string? promptVi = Clean(GetField("PromptVi"));
            string? promptViPhonetic = Clean(GetField("PromptViPhonetic"));
            string? explainEn = Clean(GetField("ExplainEn"));
            string? explainVi = Clean(GetField("ExplainVi"));
            string correctKey = Clean(GetField("CorrectOptionKey")) ?? string.Empty;

            var options = new List<AdminImportQuestionOptionItem>();
            AddOption(options, "A", GetField("A_TextEn"), GetField("A_TextVi"), 1);
            AddOption(options, "B", GetField("B_TextEn"), GetField("B_TextVi"), 2);
            AddOption(options, "C", GetField("C_TextEn"), GetField("C_TextVi"), 3);
            AddOption(options, "D", GetField("D_TextEn"), GetField("D_TextVi"), 4);

            items.Add(new AdminImportQuestionItem(
                testVersion,
                questionNo,
                type,
                promptEn,
                promptVi,
                promptViPhonetic,
                explainEn,
                explainVi,
                correctKey,
                options
            ));

            lineByIndex[items.Count - 1] = line;
        }

        return new CsvParseResult(items, lineByIndex);
    }

    private static void AddOption(
        ICollection<AdminImportQuestionOptionItem> options,
        string key,
        string? textEn,
        string? textVi,
        int sortOrder)
    {
        string? cleanEn = Clean(textEn);
        string? cleanVi = Clean(textVi);

        if (string.IsNullOrWhiteSpace(cleanEn))
        {
            return;
        }

        options.Add(new AdminImportQuestionOptionItem(key, cleanEn, cleanVi, sortOrder));
    }

    private static string? Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

}

public sealed record CsvParseResult(
    IReadOnlyList<AdminImportQuestionItem> Items,
    IReadOnlyDictionary<int, int> CsvLineByIndex
);
