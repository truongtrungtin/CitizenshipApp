using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Shared.Contracts.Me;

/// <summary>
/// Full user settings payload (used as both request/response for /api/me/settings).
/// </summary>
public sealed record UserSettings(
    [property: EnumDataType(typeof(LanguageCode))] LanguageCode Language,
    [property: EnumDataType(typeof(FontScale))] FontScale FontScale,
    [property: EnumDataType(typeof(AudioSpeed))] AudioSpeed AudioSpeed,
    [property: Range(1, 200)] int DailyGoalMinutes,
    [property: EnumDataType(typeof(StudyFocus))] StudyFocus Focus,
    bool SilentMode);