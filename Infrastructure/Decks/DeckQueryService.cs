using Application.Decks;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Shared.Contracts.Deck;

namespace Infrastructure.Decks;

/// <summary>
///     Infrastructure implementation of <see cref="IDeckQueryService" />.
///     Data source: SQL Server via EF Core (<see cref="AppDbContext" />).
///     Why this lives in Infrastructure:
///     - It knows how to load/read data.
///     - Application only defines the contract.
///     - Api controllers stay thin and testable.
/// </summary>
public sealed class DeckQueryService : IDeckQueryService
{
    private readonly AppDbContext _db;

    public DeckQueryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DeckListItem>> GetDecksAsync(CancellationToken ct)
    {
        return await _db.Decks
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new DeckListItem(
                d.DeckId,
                d.Code,
                d.Name,
                d.Questions.Count))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DeckSummary>> GetDeckSummariesAsync(CancellationToken ct)
    {
        return await _db.Decks
            .AsNoTracking()
            .OrderByDescending(d => d.IsActive)
            .ThenBy(d => d.Name)
            .Select(d => new DeckSummary
            {
                Id = d.DeckId,
                Name = d.Name,
                IsActive = d.IsActive,
                QuestionCount = d.Questions.Count
            })
            .ToListAsync(ct);
    }

    public async Task<DeckListItem?> GetDeckByIdAsync(Guid deckId, CancellationToken ct)
    {
        return await _db.Decks
            .AsNoTracking()
            .Where(d => d.DeckId == deckId)
            .Select(d => new DeckListItem(
                d.DeckId,
                d.Code,
                d.Name,
                d.Questions.Count))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Question>> GetDeckQuestionsAsync(
        Guid deckId,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        // Why: Keep validation in one place so both controllers and future callers behave consistently.
        ValidatePaging(page, pageSize);

        bool deckExists = await _db.Decks
            .AsNoTracking()
            .AnyAsync(d => d.DeckId == deckId, ct);

        if (!deckExists)
        {
            throw new KeyNotFoundException($"Deck '{deckId}' not found.");
        }

        int skip = (page - 1) * pageSize;

        // Stable paging. Order by PK for deterministic results.
        return await _db.Questions
            .AsNoTracking()
            .Where(q => q.DeckId == deckId)
            .OrderBy(q => q.QuestionId)
            .Skip(skip)
            .Take(pageSize)
            .Select(q => new Question(
                q.QuestionId,
                q.DeckId,
                QuestionTypeMapper.FromRaw(q.Type),
                new QuestionText(
                    q.PromptEn,
                    q.PromptVi ?? string.Empty,
                    q.PromptViPhonetic,
                    q.ExplainEn,
                    q.ExplainVi),
                q.Options
                    .OrderBy(o => o.SortOrder)
                    .Select(o => new AnswerOption(o.Key, o.TextEn, o.TextVi))
                    .ToList()))
            .ToListAsync(ct);
    }

    public async Task<Question?> GetQuestionByIdAsync(Guid questionId, CancellationToken ct)
    {
        return await _db.Questions
            .AsNoTracking()
            .Where(q => q.QuestionId == questionId)
            .Select(q => new Question(
                q.QuestionId,
                q.DeckId,
                QuestionTypeMapper.FromRaw(q.Type),
                new QuestionText(
                    q.PromptEn,
                    q.PromptVi ?? string.Empty,
                    q.PromptViPhonetic,
                    q.ExplainEn,
                    q.ExplainVi),
                q.Options
                    .OrderBy(o => o.SortOrder)
                    .Select(o => new AnswerOption(o.Key, o.TextEn, o.TextVi))
                    .ToList()))
            .SingleOrDefaultAsync(ct);
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
