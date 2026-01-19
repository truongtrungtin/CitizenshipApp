using Application.Study;

using Domain.Entities.Deck;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Shared.Contracts.Deck;
using Shared.Contracts.Study;

using Question = Shared.Contracts.Deck.Question;

namespace Infrastructure.Study;

/// <summary>
/// EF Core implementation of IStudyService.
/// Keeps StudyController thin and centralizes query/logic for BL-017/BL-018.
/// </summary>
public sealed class StudyService : IStudyService
{
    private readonly AppDbContext _db;

    public StudyService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<NextQuestionResponse?> GetNextQuestionAsync(Guid userId, GetNextQuestionRequest req, CancellationToken ct)
    {
        // BL-018: Optimize "random question" selection.
        // We must avoid Count+Skip for large decks.
        // IMPORTANT: Do NOT filter on a projection that materializes a collection (ToList),
        // because EF Core cannot translate that to SQL.
        //
        // Strategy:
        // 1) Pick a random Guid pivot
        // 2) Select only the QuestionId using keyset-like query (fast with index)
        // 3) Load full projection (including Options) for that single QuestionId

        Guid pivot = Guid.NewGuid();

        // Query #1: choose QuestionId only (fully translatable)
        IQueryable<Domain.Entities.Deck.Question> deckQuery = _db.Questions
            .AsNoTracking().Where(x => x.DeckId == req.DeckId);

        // Try pivot selection first
        Guid? questionId = await deckQuery
            .Where(q => q.QuestionId.CompareTo(pivot) >= 0)
            .OrderBy(q => q.QuestionId)
            .Select(q => (Guid?)q.QuestionId)
            .FirstOrDefaultAsync(ct);

        // Wrap-around if pivot landed after the last QuestionId
        questionId ??= await deckQuery
            .OrderBy(q => q.QuestionId)
            .Select(q => (Guid?)q.QuestionId)
            .FirstOrDefaultAsync(ct);

        if (questionId is null)
        {
            // Deck has no questions.
            return null;
        }

        // Query #2: load the selected question with Options (projection stays the same)
        QuestionProjection? q = await _db.Questions
            .AsNoTracking()
            .Where(x => x.DeckId == req.DeckId && x.QuestionId == questionId.Value)
            .Select(x => new QuestionProjection(
                x.QuestionId,
                x.DeckId,
                x.Type,
                x.PromptEn,
                x.PromptVi,
                x.PromptViPhonetic,
                x.ExplainEn,
                x.ExplainVi,
                x.Options
                    .OrderBy(o => o.SortOrder)
                    .Select(o => new AnswerOption(o.Key, o.TextEn, o.TextVi))
                    .ToList()
            ))
            .SingleOrDefaultAsync(ct);

        if (q is null)
        {
            return null;
        }

        var question = new Question(
            q.QuestionId,
            q.DeckId,
            QuestionTypeMapper.FromRaw(q.Type),
            new QuestionText(
                q.PromptEn,
                q.PromptVi ?? string.Empty,
                q.PromptViPhonetic,
                q.ExplainEn,
                q.ExplainVi
            ),
            q.Options
        );

        return new NextQuestionResponse { Question = question };
    }

    public async Task<SubmitAnswerResponse?> SubmitAnswerAsync(Guid userId, SubmitAnswerRequest req, CancellationToken ct)
    {
        Domain.Entities.Deck.Question? question = await _db.Questions
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.QuestionId == req.QuestionId && q.DeckId == req.DeckId, ct);

        if (question is null)
        {
            return null;
        }

        bool isCorrect = string.Equals(req.SelectedKey, question.CorrectOptionKey, StringComparison.OrdinalIgnoreCase);

        _db.StudyEvents.Add(new StudyEvent
        {
            UserId = userId,
            DeckId = req.DeckId,
            QuestionId = req.QuestionId,
            SelectedKey = req.SelectedKey,
            IsCorrect = isCorrect,
            CreatedUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);

        return new SubmitAnswerResponse
        {
            IsCorrect = isCorrect,
            CorrectKey = question.CorrectOptionKey
        };
    }

    public async Task<TodayProgressResponse> GetTodayProgressAsync(Guid userId, CancellationToken ct)
    {
        // Current behavior (MVP): UTC boundary
        DateTime startUtc = DateTime.UtcNow.Date;

        int total = await _db.StudyEvents
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.CreatedUtc >= startUtc)
            .CountAsync(ct);

        int correct = await _db.StudyEvents
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.CreatedUtc >= startUtc && x.IsCorrect)
            .CountAsync(ct);

        // DailyGoalMinutes from UserSettings (MVP)
        int goal = await _db.UserSettings
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .Select(s => s.DailyGoalMinutes)
            .FirstOrDefaultAsync(ct);

        int dailyGoal = goal <= 0 ? 10 : goal;

        return new TodayProgressResponse
        {
            DailyGoalMinutes = dailyGoal,
            TotalAnswered = total,
            CorrectAnswered = correct
        };
    }

    private sealed record QuestionProjection(
        Guid QuestionId,
        Guid DeckId,
        string Type,
        string PromptEn,
        string? PromptVi,
        string? PromptViPhonetic,
        string? ExplainEn,
        string? ExplainVi,
        List<AnswerOption> Options
    );
}
