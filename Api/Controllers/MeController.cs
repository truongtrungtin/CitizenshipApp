using Api.Infrastructure;

using Domain.Entities.Users;
using Domain.Enums;

using Infrastructure.Persistence;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Shared.Contracts.Me;

using UserSettings = Domain.Entities.Users.UserSettings;

namespace Api.Controllers;

/// <summary>
///     Các endpoint "me" dành cho user đã đăng nhập.
///     Những endpoint này dùng để:
///     - Lấy trạng thái onboarding (IsOnboarded)
///     - Đọc/ghi UserSettings
///     NOTE: Mọi endpoint trong controller này đều require JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class MeController(AppDbContext db) : ApiControllerBase
{
    /// <summary>
    ///     GET /api/me/profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<ActionResult<MeProfileResponse>> GetProfile()
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized();
        }

        UserProfile? profile = await db.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile is null)
        {
            return NotFound();
        }

        // Identity email nằm trong db.Users (IdentityDbContext)
        string? email = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync();

        return Ok(new MeProfileResponse
        {
            UserId = userId,
            Email = email ?? string.Empty,
            IsOnboarded = profile.IsOnboarded
        });
    }

    [HttpGet("settings")]
    public async Task<ActionResult<MeSettingsResponse>> GetSettings(CancellationToken ct)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized();
        }

        UserSettings? settings = await db.UserSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (settings is null)
        {
            // mặc định MVP (đồng bộ với UI)
            return Ok(new MeSettingsResponse
            {
                Language = LanguageCode.En,
                DailyGoalMinutes = 10
            });
        }

        return Ok(new MeSettingsResponse
        {
            Language = settings.Language,
            DailyGoalMinutes = settings.DailyGoalMinutes
        });
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateMeSettingsRequest req, CancellationToken ct)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized();
        }

        UserSettings? settings = await db.UserSettings
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (settings is null)
        {
            // Nếu DB chưa tạo settings (hiếm), tạo mới
            var now = DateTime.UtcNow;
            settings = new Domain.Entities.Users.UserSettings
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Language = req.Language,
                DailyGoalMinutes = req.DailyGoalMinutes,
                CreatedUtc = now,
                UpdatedUtc = now
            };
            db.UserSettings.Add(settings);
        }
        else
        {
            settings.Language = req.Language;
            settings.DailyGoalMinutes = req.DailyGoalMinutes;
            settings.UpdatedUtc = DateTime.UtcNow;
        }


        await db.SaveChangesAsync(ct);
        return Ok(new MeSettingsResponse
        {
            Language = settings.Language,
            DailyGoalMinutes = settings.DailyGoalMinutes
        });
    }

    /// <summary>
    ///     PUT /api/me/onboarding/complete
    ///     Chỉ đánh dấu hoàn tất onboarding.
    ///     (Settings đã được cập nhật qua /me/settings.)
    /// </summary>
    [HttpPut("onboarding/complete")]
    public async Task<IActionResult> CompleteOnboarding()
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized();
        }
        UserProfile? profile = await db.UserProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile is null)
        {
            return NotFound();
        }

        profile.IsOnboarded = true;
        profile.UpdatedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }

}
