using Domain.Enums;

namespace Shared.Contracts.Me;

/// <summary>
///     Request payload for PUT /api/me/settings (MVP: chỉ update phần UI đang dùng)
/// </summary>
public sealed class UpdateMeSettingsRequest
{
    public LanguageCode Language { get; set; } = LanguageCode.En;
    public int DailyGoalMinutes { get; set; } = 10;
}
