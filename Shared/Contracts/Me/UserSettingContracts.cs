using System.ComponentModel.DataAnnotations;

using Domain.Enums;

namespace Shared.Contracts.Me;

/// <summary>
/// Full user settings payload (request/response).
/// NOTE:
/// - Blazor @bind requires writable properties (get; set;).
/// - DataAnnotations validation works on these properties.
/// </summary>
public sealed record UserSettingContracts
{
    [EnumDataType(typeof(LanguageCode))]
    public LanguageCode Language { get; set; } = LanguageCode.En;

    [EnumDataType(typeof(LanguageCode))]
    public LanguageCode SystemLanguage { get; set; } = LanguageCode.Vi;

    [EnumDataType(typeof(FontScale))]
    public FontScale FontScale { get; set; } = FontScale.Large;

    [EnumDataType(typeof(AudioSpeed))]
    public AudioSpeed AudioSpeed { get; set; } = AudioSpeed.Slow;

    [Range(1, 200)]
    public int DailyGoalMinutes { get; set; } = 15;

    [EnumDataType(typeof(StudyFocus))]
    public StudyFocus Focus { get; set; } = StudyFocus.Civics;

    public bool SilentMode { get; set; }

    // Optional convenience constructor (keeps your current "new(...)" usage working).
    public UserSettingContracts() { }

    public UserSettingContracts(
        LanguageCode language,
        LanguageCode systemLanguage,
        FontScale fontScale,
        AudioSpeed audioSpeed,
        int dailyGoalMinutes,
        StudyFocus focus,
        bool silentMode)
    {
        Language = language;
        SystemLanguage = systemLanguage;
        FontScale = fontScale;
        AudioSpeed = audioSpeed;
        DailyGoalMinutes = dailyGoalMinutes;
        Focus = focus;
        SilentMode = silentMode;
    }
}
