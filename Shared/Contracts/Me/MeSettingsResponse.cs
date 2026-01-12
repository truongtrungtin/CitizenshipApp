using Domain.Enums;

namespace Shared.Contracts.Me;

/// <summary>
///     Response payload for GET /api/me/settings (MVP: chỉ trả phần UI đang dùng)
/// </summary>
public sealed class MeSettingsResponse
{
    public LanguageCode Language { get; set; } = LanguageCode.En;
    public int DailyGoalMinutes { get; set; } = 10;
}
