using Domain.Enums;

namespace Shared.Contracts.Me;

/// <summary>
/// Full user settings payload (used as both request/response for /api/me/settings).
/// </summary>
public sealed record UserSettingsContract(
    LanguageCode Language,
    FontScale FontScale,
    AudioSpeed AudioSpeed,
    int DailyGoalMinutes,
    StudyFocus Focus,
    bool SilentMode);
