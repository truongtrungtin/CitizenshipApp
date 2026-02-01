using Shared.Contracts.Me;

namespace Application.Me;

/// <summary>
/// Application contract for "me" endpoints.
/// Keeps controllers thin and avoids direct EF access in Api.
/// </summary>
public interface IMeService
{
    Task<MeProfileResponse?> GetProfileAsync(Guid userId, CancellationToken ct);

    Task<bool> CompleteOnboardingAsync(Guid userId, CancellationToken ct);

    Task<UserSettingContracts> GetSettingsFullAsync(Guid userId, CancellationToken ct);

    Task<bool> UpdateSettingsFullAsync(Guid userId, UserSettingContracts req, CancellationToken ct);
}
