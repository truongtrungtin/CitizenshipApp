using Shared.Contracts.Deck;

namespace Infrastructure.QuestionBank;

/// <summary>
///     Immutable snapshot of the question bank loaded into memory.
///     Why:
///     - Query endpoints are read-only and heavily read by UI.
///     - We want O(1) lookup by QuestionId and fast grouping by DeckId.
///     - Keeping a snapshot avoids re-parsing JSON on every request.
/// </summary>
public sealed class QuestionBankSnapshot
{
    /// <summary>
    ///     Deck definitions (id/code/name) in the order they appear in JSON.
    /// </summary>
    public required IReadOnlyList<DeckDefinition> Decks { get; init; }

    /// <summary>
    ///     Questions grouped by deck id.
    /// </summary>
    public required IReadOnlyDictionary<Guid, IReadOnlyList<Question>> QuestionsByDeckId { get; init; }

    /// <summary>
    ///     Fast lookup for GET /questions/{id}.
    /// </summary>
    public required IReadOnlyDictionary<Guid, Question> QuestionsById { get; init; }
}
