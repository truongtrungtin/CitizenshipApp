using Application.Decks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Question = Shared.Contracts.Deck.Question;

namespace Api.Controllers;

/// <summary>
///     Read-only question endpoint for MVP.
///     Route required by BACKLOG #2018:
///     - GET /api/questions/{id}
/// </summary>
[ApiController]
[Route("api/questions")]
[Authorize] // Why: User-facing content should require authentication in MVP.
public sealed class QuestionsController(IDeckQueryService decks) : ControllerBase
{
    /// <summary>
    ///     GET /api/questions/{id}
    ///     Returns 404 if the question does not exist.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Question>> GetById(Guid id, CancellationToken ct)
    {
        Question? question = await decks.GetQuestionByIdAsync(id, ct);
        return question is null ? NotFound() : Ok(question);
    }
}
