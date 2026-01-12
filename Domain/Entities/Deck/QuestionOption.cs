namespace Domain.Entities.Deck;

/// <summary>
///     Option cho câu trắc nghiệm.
///     Primary key sẽ dùng (QuestionId + Key) ở EF config.
/// </summary>
public sealed class QuestionOption
{
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = default!;

    /// <summary>Ví dụ: "A", "B", "C", "D"</summary>
    public required string Key { get; set; }

    // Text (song ngữ)
    public required string TextEn { get; set; }
    public string? TextVi { get; set; }

    public int SortOrder { get; set; }
}
