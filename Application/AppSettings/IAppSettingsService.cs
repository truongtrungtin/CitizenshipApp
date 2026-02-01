using Shared.Contracts.AppSettings;

namespace Application.AppSettings;

/// <summary>
/// Application contract for AppSettings management.
/// </summary>
public interface IAppSettingsService
{
    Task<List<AppSettingDto>> GetAllAsync(CancellationToken ct);

    Task<AppSettingDto?> GetByIdAsync(int id, CancellationToken ct);

    Task<AppSettingResult> CreateAsync(CreateAppSettingRequest req, CancellationToken ct);

    Task<AppSettingResult> UpdateAsync(int id, UpdateAppSettingRequest req, CancellationToken ct);

    Task<AppSettingResult> DeleteAsync(int id, CancellationToken ct);
}
