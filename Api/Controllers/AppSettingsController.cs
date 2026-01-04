using Domain.Entities;

using Infrastructure.Persistence;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppSettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppSettingsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<List<AppSetting>> GetAll()
    {
        return await _db.AppSettings.AsNoTracking().OrderByDescending(x => x.Id).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppSetting>> Get(int id)
    {
        AppSetting? item = await _db.AppSettings.FindAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<AppSetting>> Create([FromBody] AppSetting req)
    {
        _db.AppSettings.Add(req);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = req.Id }, req);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AppSetting req)
    {
        AppSetting? item = await _db.AppSettings.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        item.Key = req.Key;
        item.Value = req.Value;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        AppSetting? item = await _db.AppSettings.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        _db.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
