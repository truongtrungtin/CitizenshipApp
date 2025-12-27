namespace Shared.Contracts;

// Why: Keep DTOs immutable + simple so both API and Blazor can share them safely.
// Using records reduces boilerplate and makes mapping predictable.

public sealed record DeckListItemDto(
    Guid Id,
    string Code,
    string Name,
    int QuestionCount
);

public sealed record AnswerOptionDto(
    string Key,          // e.g., "A", "B", "C", "D"
    string TextEn,
    string TextVi
);

public sealed record QuestionTextDto(
    string TextEn,
    string TextVi,
    string? ViPhonetic,  // optional: Vietnamese-friendly phonetics
    string? ExplainEn,   // optional: short explanation
    string? ExplainVi
);

public sealed record QuestionDto(
    Guid Id,
    Guid DeckId,
    string Type,                 // "CivicsMcq", "Reading", "Writing" (MVP)
    QuestionTextDto Prompt,
    IReadOnlyList<AnswerOptionDto> Options
);
