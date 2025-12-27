using System.Security.Claims;

using Domain.Entities.Users;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Me;

namespace Api.Controllers;

/// <summary>
/// Các endpoint "me" dành cho user đã đăng nhập.
///
/// Những endpoint này dùng để:
/// - Lấy trạng thái onboarding (IsOnboarded)
/// - Đọc/ghi UserSettings
///
/// NOTE: Mọi endpoint trong controller này đều require JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class MeController : ControllerBase
{
    private readonly AppDbContext _db;

    public MeController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// GET /api/me/profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var userId = GetUserIdOrThrow();
        var profile = await _db.UserProfiles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile is null)
            return NotFound();

        return Ok(new UserProfileDto(profile.IsOnboarded));
    }

    /// <summary>
    /// GET /api/me/settings
    /// </summary>
    [HttpGet("settings")]
    public async Task<ActionResult<UserSettingsDto>> GetSettings()
    {
        var userId = GetUserIdOrThrow();
        var settings = await _db.UserSettings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (settings is null)
            return NotFound();

        return Ok(new UserSettingsDto(
            settings.Language,
            settings.FontScale,
            settings.AudioSpeed,
            settings.DailyGoalMinutes,
            settings.Focus,
            settings.SilentMode));
    }

    /// <summary>
    /// PUT /api/me/settings
    ///
    /// Dùng cho onboarding + settings page.
    /// </summary>
    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] UserSettingsDto req)
    {
        var userId = GetUserIdOrThrow();
        var settings = await _db.UserSettings
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (settings is null)
            return NotFound();

        settings.Language = req.Language;
        settings.FontScale = req.FontScale;
        settings.AudioSpeed = req.AudioSpeed;
        settings.DailyGoalMinutes = req.DailyGoalMinutes;
        settings.Focus = req.Focus;
        settings.SilentMode = req.SilentMode;
        settings.UpdatedUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// PUT /api/me/onboarding/complete
    ///
    /// Chỉ đánh dấu hoàn tất onboarding.
    /// (Settings đã được cập nhật qua /me/settings.)
    /// </summary>
    [HttpPut("onboarding/complete")]
    public async Task<IActionResult> CompleteOnboarding()
    {
        var userId = GetUserIdOrThrow();
        var profile = await _db.UserProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile is null)
            return NotFound();

        profile.IsOnboarded = true;
        profile.UpdatedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Lấy Guid userId từ JWT claim (sub).
    /// </summary>
    private Guid GetUserIdOrThrow()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(ClaimTypes.Name)
                  ?? User.FindFirstValue("sub");

        if (sub is null || !Guid.TryParse(sub, out var userId))
        {
            // Nếu token không có sub hoặc parse fail => token invalid.
            throw new InvalidOperationException("Invalid JWT: missing/invalid user id.");
        }

        return userId;
    }
}
