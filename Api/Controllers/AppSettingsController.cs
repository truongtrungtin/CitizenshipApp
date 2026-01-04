using Infrastructure.Persistence;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Shared.Contracts.AppSettings;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AppSettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppSettingsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<List<AppSettingDto>> GetAll()
    {
        return await _db.AppSettings
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Select(x => new AppSettingDto(x.Id, x.Key, x.Value))
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppSettingDto>> Get(int id)
    {
        var item = await _db.AppSettings
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AppSettingDto(x.Id, x.Key, x.Value))
            .SingleOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<AppSettingDto>> Create([FromBody] CreateAppSettingRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Key))
        {
            return BadRequest("Key is required.");
        }

        bool exists = await _db.AppSettings.AsNoTracking().AnyAsync(x => x.Key == req.Key);
        if (exists)
        {
            return Conflict("Key already exists.");
        }

        var entity = new Domain.Entities.AppSetting
        {
            Key = req.Key,
            Value = req.Value
        };

        _db.AppSettings.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new AppSettingDto(entity.Id, entity.Key, entity.Value));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAppSettingRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Key))
        {
            return BadRequest("Key is required.");
        }

        Domain.Entities.AppSetting? item = await _db.AppSettings.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        bool keyInUseByOther = await _db.AppSettings.AsNoTracking().AnyAsync(x => x.Key == req.Key && x.Id != id);
        if (keyInUseByOther)
        {
            return Conflict("Key already exists.");
        }

        item.Key = req.Key;
        item.Value = req.Value;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        Domain.Entities.AppSetting? item = await _db.AppSettings.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        _db.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
