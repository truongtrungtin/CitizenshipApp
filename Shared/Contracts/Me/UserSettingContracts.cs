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

    [EnumDataType(typeof(FontScale))]
    public FontScale FontScale { get; set; } = FontScale.Medium;

    [EnumDataType(typeof(AudioSpeed))]
    public AudioSpeed AudioSpeed { get; set; } = AudioSpeed.Normal;

    [Range(1, 200)]
    public int DailyGoalMinutes { get; set; } = 15;

    [EnumDataType(typeof(StudyFocus))]
    public StudyFocus Focus { get; set; } = StudyFocus.Civics;

    public bool SilentMode { get; set; } = false;

    // Optional convenience constructor (keeps your current "new(...)" usage working).
    public UserSettingContracts() { }

    public UserSettingContracts(
        LanguageCode language,
        FontScale fontScale,
        AudioSpeed audioSpeed,
        int dailyGoalMinutes,
        StudyFocus focus,
        bool silentMode)
    {
        Language = language;
        FontScale = fontScale;
        AudioSpeed = audioSpeed;
        DailyGoalMinutes = dailyGoalMinutes;
        Focus = focus;
        SilentMode = silentMode;
    }
}
