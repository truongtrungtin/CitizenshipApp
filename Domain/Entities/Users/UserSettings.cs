using Domain.Enums;

namespace Domain.Entities.Users;

/// <summary>
///     UserSettings lưu các cấu hình UI/UX cho người lớn tuổi.
///     Lưu ý:
///     - Mặc định chọn các giá trị thân thiện (font lớn, audio chậm).
///     - Sau này onboarding sẽ cập nhật các giá trị này.
/// </summary>
public sealed class UserSettings
{
    /// <summary>
    ///     Primary key của UserSettings.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    ///     FK trỏ về Identity user (AppUser.Id).
    ///     Dự kiến: mỗi user có đúng 1 settings row.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    ///     Ngôn ngữ UI.
    /// </summary>
    public LanguageCode Language { get; set; } = LanguageCode.Vi;

    /// <summary>
    ///     Cỡ chữ UI.
    /// </summary>
    public FontScale FontScale { get; set; } = FontScale.Large;

    /// <summary>
    ///     Tốc độ audio.
    /// </summary>
    public AudioSpeed AudioSpeed { get; set; } = AudioSpeed.Slow;

    /// <summary>
    ///     Mục tiêu học mỗi ngày (phút).
    /// </summary>
    public int DailyGoalMinutes { get; set; } = 15;

    /// <summary>
    ///     Nội dung ưu tiên học.
    /// </summary>
    public StudyFocus Focus { get; set; } = StudyFocus.Civics;

    /// <summary>
    ///     Silent mode: người dùng không thể nghe/nói (chỉ làm MCQ).
    ///     UI/logic session sẽ dựa vào flag này.
    /// </summary>
    public bool SilentMode { get; set; } = false;

    /// <summary>
    ///     Audit timestamps (UTC).
    /// </summary>
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}
