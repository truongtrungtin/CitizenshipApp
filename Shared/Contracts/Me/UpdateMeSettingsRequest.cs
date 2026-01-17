using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Shared.Contracts.Me;

/// <summary>
/// Request payload for PUT /api/me/settings (MVP: only fields used by UI)
/// </summary>
public sealed class UpdateMeSettingsRequest
{
    [EnumDataType(typeof(LanguageCode))]
    public LanguageCode Language { get; set; } = LanguageCode.En;

    // Match UI constraints (Settings page currently limits 1..200)
    [Range(1, 200)]
    public int DailyGoalMinutes { get; set; } = 10;
}