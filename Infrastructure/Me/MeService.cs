using Application.Me;

using Domain.Enums;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Shared.Contracts.Me;

namespace Infrastructure.Me;

/// <summary>
/// Infrastructure implementation for "me" endpoints.
/// Why:
/// - Centralize EF Core logic outside controllers.
/// - Keep API surface thin and testable.
/// </summary>
public sealed class MeService(AppDbContext db) : IMeService
{
    public async Task<MeProfileResponse?> GetProfileAsync(Guid userId, CancellationToken ct)
    {
        var profile = await db.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (profile is null)
        {
            return null;
        }

        string? email = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(ct);

        return new MeProfileResponse
        {
            UserId = userId,
            Email = email ?? string.Empty,
            IsOnboarded = profile.IsOnboarded
        };
    }

    public async Task<bool> CompleteOnboardingAsync(Guid userId, CancellationToken ct)
    {
        var profile = await db.UserProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (profile is null)
        {
            return false;
        }

        profile.IsOnboarded = true;
        profile.UpdatedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<UserSettingContracts> GetSettingsFullAsync(Guid userId, CancellationToken ct)
    {
        var settings = await db.UserSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (settings is null)
        {
            // Why: provide safe defaults if settings row is missing.
            return new UserSettingContracts(
                language: LanguageCode.En,
                systemLanguage: LanguageCode.Vi,
                fontScale: FontScale.Large,
                audioSpeed: AudioSpeed.Slow,
                dailyGoalMinutes: 15,
                focus: StudyFocus.Civics,
                silentMode: false
            );
        }

        return new UserSettingContracts(
            language: settings.Language,
            systemLanguage: settings.SystemLanguage,
            fontScale: settings.FontScale,
            audioSpeed: settings.AudioSpeed,
            dailyGoalMinutes: settings.DailyGoalMinutes,
            focus: settings.Focus,
            silentMode: settings.SilentMode
        );
    }

    public async Task<bool> UpdateSettingsFullAsync(Guid userId, UserSettingContracts req, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var settings = await db.UserSettings
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (settings is null)
        {
            // IMPORTANT: UserSettings uses shared PK with UserProfile (Settings.Id == Profile.Id)
            var profile = await db.UserProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId, ct);

            if (profile is null)
            {
                return false;
            }

            settings = new Domain.Entities.Users.UserSettings
            {
                Id = profile.Id,
                UserId = userId,
                CreatedUtc = now,
                UpdatedUtc = now
            };

            db.UserSettings.Add(settings);
        }

        settings.Language = req.Language;
        settings.SystemLanguage = req.SystemLanguage;
        settings.FontScale = req.FontScale;
        settings.AudioSpeed = req.AudioSpeed;
        settings.DailyGoalMinutes = req.DailyGoalMinutes;
        settings.Focus = req.Focus;
        settings.SilentMode = req.SilentMode;
        settings.UpdatedUtc = now;

        await db.SaveChangesAsync(ct);
        return true;
    }
}
