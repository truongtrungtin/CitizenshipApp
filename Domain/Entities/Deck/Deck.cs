namespace Domain.Entities.Deck;

/// <summary>
///     Deck = một bộ câu hỏi (VD: Civics 100, Vocabulary, Reading...)
///     Code dùng để seed/lookup ổn định, không phụ thuộc Guid.
/// </summary>
public sealed class Deck
{
    public Guid DeckId { get; set; } = Guid.NewGuid();

    /// <summary>Ví dụ: "civics-100-en"</summary>
    public required string Code { get; set; }

    /// <summary>Ví dụ: "US Civics (100 Questions)"</summary>
    public required string Name { get; set; }

    public bool IsActive { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
