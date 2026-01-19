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
        // Current behavior (MVP): random 1 question in deck using Count + Skip.
        // BL-018 will optimize this logic later (without changing controller).
        int total = await _db.Questions
            .AsNoTracking()
            .Where(q => q.DeckId == req.DeckId)
            .CountAsync(ct);

        if (total == 0)
        {
            return null;
        }

        int skip = Random.Shared.Next(0, total);

        var q = await _db.Questions
            .AsNoTracking()
            .Where(x => x.DeckId == req.DeckId)
            .OrderBy(x => x.QuestionId) // stable order so Skip is deterministic
            .Skip(skip)
            .Select(x => new
            {
                x.QuestionId,
                x.DeckId,
                x.Type,
                x.PromptEn,
                x.PromptVi,
                x.PromptViPhonetic,
                x.ExplainEn,
                x.ExplainVi,
                Options = x.Options
                    .OrderBy(o => o.SortOrder)
                    .Select(o => new AnswerOption(o.Key, o.TextEn, o.TextVi))
                    .ToList()
            })
            .FirstAsync(ct);

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
}
