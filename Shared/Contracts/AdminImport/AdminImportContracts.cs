namespace Shared.Contracts.AdminImport;

public sealed record AdminImportQuestionsRequest(
    IReadOnlyList<AdminImportQuestionItem> Items
);

public sealed record AdminImportQuestionItem(
    string TestVersion,
    int QuestionNo,
    string Type,
    string PromptEn,
    string? PromptVi,
    string? PromptViPhonetic,
    string? ExplainEn,
    string? ExplainVi,
    string CorrectOptionKey,
    IReadOnlyList<AdminImportQuestionOptionItem>? Options
);

public sealed record AdminImportQuestionOptionItem(
    string Key,
    string TextEn,
    string? TextVi,
    int SortOrder
);

public sealed record ImportError(
    int? Index,
    int? CsvLine,
    string? Field,
    string Message
);

public sealed record AdminImportQuestionsResult(
    int CreatedCount,
    int UpdatedCount,
    int SkippedCount,
    int ErrorCount,
    IReadOnlyList<ImportError> Errors
);
