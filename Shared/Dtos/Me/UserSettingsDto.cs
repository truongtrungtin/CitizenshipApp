using Domain.Enums;

namespace Shared.Dtos.Me;

/// <summary>
/// DTO cho user settings (onboarding + settings screen).
///
/// NOTE:
/// - Dùng enum từ Domain để tránh mismatch.
/// - UI sẽ map enum -> label (VN/EN) theo UX rules.
/// </summary>
public sealed record UserSettingsDto(
    LanguageCode Language,
    FontScale FontScale,
    AudioSpeed AudioSpeed,
    int DailyGoalMinutes,
    StudyFocus Focus,
    bool SilentMode);
