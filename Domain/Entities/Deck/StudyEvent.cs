namespace Domain.Entities.Deck;

/// <summary>
///     Lưu log mỗi lần user trả lời: để làm Home progress + thống kê.
///     MVP dùng log đơn giản (không spaced repetition).
/// </summary>
public sealed class StudyEvent
{
    public Guid StudyEventId { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public Guid DeckId { get; set; }
    public Guid QuestionId { get; set; }

    public required string SelectedKey { get; set; }
    public bool IsCorrect { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
