namespace Domain.Entities.Users;

/// <summary>
/// UserProfile lưu các thông tin “hồ sơ học tập” gắn với user (Identity user).
///
/// Nguyên tắc:
/// - Domain entity KHÔNG phụ thuộc EF Core attributes.
/// - Mapping/Indexes sẽ làm ở Infrastructure bằng Fluent Configuration.
/// </summary>
public sealed class UserProfile
{
    /// <summary>
    /// Primary key của UserProfile.
    /// (Tách riêng với Identity UserId để linh hoạt về sau)
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// FK trỏ về Identity user (AppUser.Id).
    /// Đây là cột quan trọng để join profile với user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Onboarding là bắt buộc.
    /// - false: user phải đi qua /onboarding
    /// - true: user vào /home bình thường
    /// </summary>
    public bool IsOnboarded { get; set; } = false;

    /// <summary>
    /// Thời điểm tạo profile (UTC).
    /// </summary>
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời điểm cập nhật profile lần cuối (UTC).
    /// </summary>
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}
