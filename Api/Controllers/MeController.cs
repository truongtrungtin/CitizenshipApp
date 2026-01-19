using Api.Infrastructure;

using Domain.Entities.Users;
using Domain.Enums;

using Infrastructure.Persistence;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Shared.Contracts.Me;

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
    ///     GET /api/Me/profile
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
            // AFTER (đúng shared PK: settings.Id == profile.Id)
            var profile = await db.UserProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId, ct);

            if (profile is null)
            {
                // This should not happen for a valid authenticated user.
                return NotFound();
            }

            // Nếu DB chưa tạo settings (hiếm), tạo mới
            var now = DateTime.UtcNow;

            settings = new UserSettings
            {
                Id = profile.Id, // đồng bộ PK với UserProfile
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


    [HttpGet("settings/full")]
    public async Task<ActionResult<UserSettingContracts>> GetSettingsFull(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var settings = await db.UserSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (settings is null)
        {
            return Ok(new UserSettingContracts(
                language: LanguageCode.En,
                fontScale: FontScale.Medium,
                audioSpeed: AudioSpeed.Normal,
                dailyGoalMinutes: 15,
                focus: StudyFocus.Civics,
                silentMode: false
            ));
        }

        return Ok(new UserSettingContracts(
            language: settings.Language,
            fontScale: settings.FontScale,
            audioSpeed: settings.AudioSpeed,
            dailyGoalMinutes: settings.DailyGoalMinutes,
            focus: settings.Focus,
            silentMode: settings.SilentMode
        ));
    }

    [HttpPut("settings/full")]
    public async Task<IActionResult> UpdateSettingsFull([FromBody] UserSettingContracts req, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var now = DateTime.UtcNow;

        var settings = await db.UserSettings
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (settings is null)
        {
            // IMPORTANT: UserSettings uses shared PK with UserProfile (Settings.Id == Profile.Id)
            var profile = await db.UserProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId, ct);

            if (profile is null)
                return NotFound();

            settings = new Domain.Entities.Users.UserSettings
            {
                Id = profile.Id,   // shared PK
                UserId = userId,
                CreatedUtc = now,
                UpdatedUtc = now
            };

            db.UserSettings.Add(settings);
        }

        settings.Language = req.Language;
        settings.FontScale = req.FontScale;
        settings.AudioSpeed = req.AudioSpeed;
        settings.DailyGoalMinutes = req.DailyGoalMinutes;
        settings.Focus = req.Focus;
        settings.SilentMode = req.SilentMode;
        settings.UpdatedUtc = now;

        await db.SaveChangesAsync(ct);
        return NoContent();
    }



}
