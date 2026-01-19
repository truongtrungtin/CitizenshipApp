using Api.Infrastructure;

using Application.Study;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shared.Contracts.Study;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class StudyController : ApiControllerBase
{
    private readonly IStudyService _study;

    public StudyController(IStudyService study)
    {
        _study = study;
    }

    [HttpGet("next")]
    public async Task<ActionResult<NextQuestionResponse>> GetNext([FromQuery] GetNextQuestionRequest req, CancellationToken ct)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized();
        }

        NextQuestionResponse? next = await _study.GetNextQuestionAsync(userId, req, ct);
        if (next is null)
        {
            return NotFound("Deck has no questions.");
        }

        return Ok(next);
    }

    [HttpPost("answer")]
    public async Task<ActionResult<SubmitAnswerResponse>> SubmitAnswer([FromBody] SubmitAnswerRequest req, CancellationToken ct)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized();
        }

        SubmitAnswerResponse? result = await _study.SubmitAnswerAsync(userId, req, ct);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("today")]
    public async Task<ActionResult<TodayProgressResponse>> Today(CancellationToken ct)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized();
        }

        TodayProgressResponse result = await _study.GetTodayProgressAsync(userId, ct);
        return Ok(result);
    }
}
