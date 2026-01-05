using Shared.Contracts.Deck;

namespace Application.Decks;

/// <summary>
///     Read-only query service for Decks and Questions.
///     Why:
///     - EPIC 4 is read-only first: we need simple endpoints for UI browsing/preview.
///     - Keep controllers thin: controller -> Application interface -> Infrastructure implementation.
///     - This interface lives in Application so Api does not depend on Infrastructure/EF directly.
///     Notes:
///     - Return types are shared DTOs so Blazor and API stay in sync.
///     - Write/admin endpoints will be added later under EPIC 5.
/// </summary>
public interface IDeckQueryService
{
    /// <summary>
    ///     Get a list of decks for the deck picker UI.
    /// </summary>
    Task<IReadOnlyList<DeckSummary>> GetDeckSummariesAsync(CancellationToken ct);

    /// <summary>
    ///     Get a list of decks available in the question bank.
    /// </summary>
    Task<IReadOnlyList<DeckListItem>> GetDecksAsync(CancellationToken ct);

    /// <summary>
    ///     Get a single deck by its id.
    ///     Returns null if not found.
    /// </summary>
    Task<DeckListItem?> GetDeckByIdAsync(Guid deckId, CancellationToken ct);

    /// <summary>
    ///     Get paged questions for a specific deck.
    ///     Expected behavior:
    ///     - Throws <see cref="KeyNotFoundException" /> if <paramref name="deckId" /> does not exist.
    ///     - Validates <paramref name="page" /> and <paramref name="pageSize" />.
    /// </summary>
    Task<IReadOnlyList<Question>> GetDeckQuestionsAsync(Guid deckId, int page, int pageSize, CancellationToken ct);

    /// <summary>
    ///     Get a single question by its id.
    ///     Returns null if not found.
    /// </summary>
    Task<Question?> GetQuestionByIdAsync(Guid questionId, CancellationToken ct);
}
