using Api.Infrastructure;

using Application.Study;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shared.Contracts.Study;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class StudyController(IStudyService study) : ApiControllerBase
{

    [HttpGet("next")]
    public async Task<ActionResult<NextQuestionResponse>> GetNext([FromQuery] GetNextQuestionRequest req, CancellationToken ct)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Missing or invalid access token.");
        }

        NextQuestionResponse? next = await study.GetNextQuestionAsync(userId, req, ct);
        if (next is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: "Deck has no questions.");
        }

        return Ok(next);
    }

    [HttpPost("answer")]
    public async Task<ActionResult<SubmitAnswerResponse>> SubmitAnswer([FromBody] SubmitAnswerRequest req, CancellationToken ct)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Missing or invalid access token.");
        }

        SubmitAnswerResponse? result = await study.SubmitAnswerAsync(userId, req, ct);
        if (result is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: "Question not found.");
        }

        return Ok(result);
    }

    [HttpGet("today")]
    public async Task<ActionResult<TodayProgressResponse>> Today(CancellationToken ct)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Missing or invalid access token.");
        }

        TodayProgressResponse result = await study.GetTodayProgressAsync(userId, ct);
        return Ok(result);
    }
}
