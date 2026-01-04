namespace Infrastructure.QuestionBank;

/// <summary>
///     Abstraction for obtaining the question bank snapshot.
///     Why:
///     - Keeps the rest of Infrastructure (DeckQueryService) agnostic of where data comes from.
///     - In MVP we load from an embedded JSON resource; later we can swap to DB/CSV import with minimal change.
/// </summary>
public interface IQuestionBankStore
{
    /// <summary>
    ///     Get the cached snapshot (loads it once on first use).
    /// </summary>
    Task<QuestionBankSnapshot> GetSnapshotAsync(CancellationToken ct);
}
