using Api.Infrastructure;

using Domain.Entities.Deck;

using Infrastructure.Persistence;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Shared.Contracts.Deck;
using Shared.Contracts.Study;

using Question = Shared.Contracts.Deck.Question;

namespace Api.Controllers;

[ApiController]
[Route("api/study")]
[Authorize]
public sealed class StudyController : ApiControllerBase
{
    private readonly AppDbContext _db;

    public StudyController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("next")]
    public async Task<ActionResult<NextQuestionResponse>> GetNext([FromQuery] Guid deckId, CancellationToken ct)
    {
        // MVP: random 1 câu trong deck
        int total = await _db.Questions.Where(q => q.DeckId == deckId).CountAsync(ct);
        if (total == 0)
        {
            return NotFound("Deck has no questions.");
        }

        int skip = Random.Shared.Next(0, total);

        var q = await _db.Questions
            .AsNoTracking()
            .Where(x => x.DeckId == deckId)
            .OrderBy(x => x.QuestionId) // stable order để Skip hoạt động ổn
            .Skip(skip)
            .Select(x => new
            {
                x.QuestionId,
                x.DeckId,
                x.Type, // string trong DB
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

        // Map theo đúng Shared.Contracts.Deck.Question (5 fields)
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

        return Ok(new NextQuestionResponse { Question = question });
    }

    [HttpPost("answer")]
    public async Task<ActionResult<SubmitAnswerResponse>> SubmitAnswer([FromBody] SubmitAnswerRequest req,
        CancellationToken ct)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized();
        }

        Domain.Entities.Deck.Question? question = await _db.Questions
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.QuestionId == req.QuestionId && q.DeckId == req.DeckId, ct);

        if (question is null)
        {
            return NotFound();
        }

        bool isCorrect = string.Equals(req.SelectedKey, question.CorrectOptionKey, StringComparison.OrdinalIgnoreCase);

        // MVP progress log
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

        return Ok(new SubmitAnswerResponse
        {
            IsCorrect = isCorrect,
            CorrectKey = question.CorrectOptionKey
        });
    }

    [HttpGet("today")]
    public async Task<ActionResult<TodayProgressResponse>> Today(CancellationToken ct)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized();
        }

        // MVP: UTC boundary
        DateTime startUtc = DateTime.UtcNow.Date;

        int total = await _db.StudyEvents
            .Where(x => x.UserId == userId && x.CreatedUtc >= startUtc)
            .CountAsync(ct);

        int correct = await _db.StudyEvents
            .Where(x => x.UserId == userId && x.CreatedUtc >= startUtc && x.IsCorrect)
            .CountAsync(ct);

        // DailyGoalMinutes: lấy từ UserSettings entity (MVP)
        int goal = await _db.UserSettings
            .Where(s => s.UserId == userId)
            .Select(s => s.DailyGoalMinutes)
            .FirstOrDefaultAsync(ct);

        int dailyGoal = goal <= 0 ? 10 : goal;

        return Ok(new TodayProgressResponse
        {
            DailyGoalMinutes = dailyGoal,
            TotalAnswered = total,
            CorrectAnswered = correct
        });
    }

}
