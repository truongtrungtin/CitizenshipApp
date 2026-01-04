using Domain.Entities.Deck;

using Infrastructure.Persistence;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Shared.Contracts.Deck;

namespace Api.Controllers;

[ApiController]
[Route("api/decks")]
[Authorize]
public sealed class DecksController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DeckSummary>>> GetAll(CancellationToken ct)
    {
        var items = await db.Decks
            .AsNoTracking()
            .OrderByDescending(d => d.IsActive)
            .ThenBy(d => d.Name)
            .Select(d => new DeckSummary
            {
                Id = d.DeckId,      // hoặc d.Id tuỳ entity bạn đặt tên
                Name = d.Name,
                IsActive = d.IsActive
            })
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("{deckId:guid}")]
    public async Task<ActionResult<DeckListItem>> GetDeck(Guid deckId, CancellationToken ct)
    {
        Deck? deck = await db.Decks
            .AsNoTracking().Include(deck => deck.Questions)
            .FirstOrDefaultAsync(d => d.DeckId == deckId, ct);

        if (deck is null)
        {
            return NotFound();
        }

        return Ok(new DeckListItem(deck.DeckId, deck.Code, deck.Name, deck.Questions.Count));
    }
}
