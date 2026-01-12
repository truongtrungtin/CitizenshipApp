namespace Shared.Contracts.Deck;

/// <summary>
/// Item dùng cho màn hình chọn Deck (list ngắn).
/// </summary>
public sealed class DeckSummary
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // Optional (nếu bạn muốn show số câu hỏi):
    public int QuestionCount { get; set; }
}
