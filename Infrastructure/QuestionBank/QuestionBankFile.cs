using Shared.Contracts.Deck;

namespace Infrastructure.QuestionBank;

/// <summary>
///     Internal DTO used to deserialize the embedded JSON question bank.
///     Why:
///     - We keep the file format explicit instead of deserializing into a raw dictionary.
///     - It allows validation and evolution of the format later (admin import, versioning, etc.).
///     Notes:
///     - <see cref="QuestionDto" /> comes from Shared.Contracts so the API and UI get identical shapes.
///     - DeckListItemDto includes QuestionCount, which is computed at runtime (not stored in JSON).
/// </summary>
internal sealed class QuestionBankFile
{
    /// <summary>
    ///     Deck metadata (id/code/name).
    /// </summary>
    public List<DeckDefinition> Decks { get; init; } = new();

    /// <summary>
    ///     All questions in the bank.
    ///     Each question includes DeckId so we can group them per deck.
    /// </summary>
    public List<Question> Questions { get; init; } = new();
}

/// <summary>
///     Minimal deck definition stored in the JSON file.
/// </summary>
public sealed record DeckDefinition(
    Guid Id,
    string Code,
    string Name
);
