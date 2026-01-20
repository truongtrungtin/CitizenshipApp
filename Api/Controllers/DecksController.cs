using Application.Decks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shared.Contracts.Deck;

namespace Api.Controllers;

[ApiController]
[Route("api/decks")]
[Authorize]
public sealed class DecksController(IDeckQueryService decks) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DeckSummary>>> GetAll(CancellationToken ct)
    {
        IReadOnlyList<DeckSummary> items = await decks.GetDeckSummariesAsync(ct);

        return Ok(items);
    }

    [HttpGet("{deckId:guid}")]
    public async Task<ActionResult<DeckListItem>> GetDeck(Guid deckId, CancellationToken ct)
    {
        DeckListItem? deck = await decks.GetDeckByIdAsync(deckId, ct);
        return deck is null ? NotFound() : Ok(deck);
    }


    [HttpGet("{deckId:guid}/questions")]
    public async Task<ActionResult<List<Question>>> GetDeckQuestions(Guid deckId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        IReadOnlyList<Question> items = await decks.GetDeckQuestionsAsync(deckId, page, pageSize, ct);
        return Ok(items);
    }
}
