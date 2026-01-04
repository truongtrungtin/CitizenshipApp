namespace Domain.Entities.Deck;

/// <summary>
///     Question = câu hỏi trong deck.
///     Type là string để sau này mở rộng (MCQ / FreeText / Listening / Speaking...).
/// </summary>
public sealed class Question
{
    public Guid QuestionId { get; set; } = Guid.NewGuid();

    public Guid DeckId { get; set; }
    public Deck Deck { get; set; } = default!;

    /// <summary>Ví dụ: "MCQ"</summary>
    public required string Type { get; set; }

    // Prompt (song ngữ)
    public required string PromptEn { get; set; }
    public string? PromptVi { get; set; }
    public string? PromptViPhonetic { get; set; }

    // Explain (song ngữ) - giải thích sau khi trả lời
    public string? ExplainEn { get; set; }
    public string? ExplainVi { get; set; }

    /// <summary>
    ///     Đáp án đúng: key của option (A/B/C/D...).
    ///     Không trả về trong QuestionDto để tránh lộ đáp án trước.
    /// </summary>
    public required string CorrectOptionKey { get; set; }

    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
}
