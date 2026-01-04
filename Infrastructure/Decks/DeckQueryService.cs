using Application.Decks;

using Infrastructure.QuestionBank;

using Shared.Contracts.Deck;

namespace Infrastructure.Decks;

/// <summary>
///     Infrastructure implementation of <see cref="IDeckQueryService" />.
///     Data source (MVP): <see cref="EmbeddedQuestionBankStore" /> loads an embedded JSON question bank.
///     Why this lives in Infrastructure:
///     - It knows how to load/read data.
///     - Application only defines the contract.
///     - Api controllers stay thin and testable.
/// </summary>
public sealed class DeckQueryService : IDeckQueryService
{
    private readonly IQuestionBankStore _store;

    public DeckQueryService(IQuestionBankStore store)
    {
        _store = store;
    }

    public async Task<IReadOnlyList<DeckListItem>> GetDecksAsync(CancellationToken ct)
    {
        QuestionBankSnapshot snapshot = await _store.GetSnapshotAsync(ct);

        // Why: QuestionCount is derived from the question bank snapshot.
        // We keep it computed here so the JSON file stays simple.
        var result = new List<DeckListItem>(snapshot.Decks.Count);

        foreach (DeckDefinition deck in snapshot.Decks)
        {
            snapshot.QuestionsByDeckId.TryGetValue(deck.Id, out IReadOnlyList<Question>? questions);
            int count = questions?.Count ?? 0;

            result.Add(new DeckListItem(
                deck.Id,
                deck.Code,
                deck.Name,
                count));
        }

        return result;
    }

    public async Task<IReadOnlyList<Question>> GetDeckQuestionsAsync(
        Guid deckId,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        // Why: Keep validation in one place so both controllers and future callers behave consistently.
        ValidatePaging(page, pageSize);

        QuestionBankSnapshot snapshot = await _store.GetSnapshotAsync(ct);

        if (!snapshot.QuestionsByDeckId.TryGetValue(deckId, out IReadOnlyList<Question>? questions))
        {
            // The deck exists but has zero questions, or the deck id does not exist.
            // We decide that "missing deck" should be 404, while an empty deck should still be valid.
            // Because the snapshot is built from JSON, if a deck exists it will appear in snapshot.Decks.
            bool deckExists = snapshot.Decks.Any(d => d.Id == deckId);
            if (!deckExists)
            {
                throw new KeyNotFoundException($"Deck '{deckId}' not found.");
            }

            return Array.Empty<Question>();
        }

        // Stable paging.
        // Note: Questions are currently in the order they appear in JSON.
        // If you want a deterministic sort independent of file order, add an Order field and sort by it.
        int skip = (page - 1) * pageSize;
        return questions.Skip(skip).Take(pageSize).ToList();
    }

    public async Task<Question?> GetQuestionByIdAsync(Guid questionId, CancellationToken ct)
    {
        QuestionBankSnapshot snapshot = await _store.GetSnapshotAsync(ct);
        return snapshot.QuestionsById.TryGetValue(questionId, out Question? q) ? q : null;
    }

    private static void ValidatePaging(int page, int pageSize)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "page must be >= 1");
        }

        // Why: Keep pageSize bounded to avoid accidental huge payloads.
        if (pageSize < 1 || pageSize > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "pageSize must be between 1 and 200");
        }
    }
}
