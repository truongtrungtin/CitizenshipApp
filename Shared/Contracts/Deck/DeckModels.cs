namespace Shared.Contracts.Deck;

// Why: Keep contracts immutable + simple so both API and Blazor can share them safely.
// Records reduce boilerplate and make mapping predictable.

public sealed record DeckListItem(
    Guid Id,
    string Code,
    string Name,
    int QuestionCount
);

public sealed record AnswerOption(
    string Key, // e.g., "A", "B", "C", "D"
    string TextEn,
    string? TextVi
);

public sealed record QuestionText(
    string TextEn,
    string TextVi,
    string? ViPhonetic, // optional: Vietnamese-friendly phonetics
    string? ExplainEn, // optional: short explanation
    string? ExplainVi
);

public sealed record Question(
    Guid Id,
    Guid DeckId,
    QuestionType Type, // "CivicsMcq", "Reading", "Writing" (MVP)
    QuestionText Prompt,
    IReadOnlyList<AnswerOption> Options
);
