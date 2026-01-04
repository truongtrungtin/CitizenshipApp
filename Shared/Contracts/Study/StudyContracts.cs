using Shared.Contracts.Deck;

namespace Shared.Contracts.Study;

/// <summary>
///     Response payload for GET /api/study/next
/// </summary>
public sealed class NextQuestionResponse
{
    public Question Question { get; set; } = default!;
}

/// <summary>
///     Request payload for POST /api/study/answer
/// </summary>
public sealed class SubmitAnswerRequest
{
    public Guid DeckId { get; set; }
    public Guid QuestionId { get; set; }
    public string SelectedKey { get; set; } = string.Empty;
}

/// <summary>
///     Response payload for POST /api/study/answer
/// </summary>
public sealed class SubmitAnswerResponse
{
    public bool IsCorrect { get; set; }

    /// <summary>
    ///     Trả về key đúng để UI highlight khi user chọn sai.
    ///     (Có thể null nếu bạn muốn chỉ trả khi sai, nhưng hiện tại API có thể luôn trả để UI đơn giản hơn.)
    /// </summary>
    public string? CorrectKey { get; set; }
}

/// <summary>
///     Response payload for GET /api/study/today
/// </summary>
public sealed class TodayProgressResponse
{
    public int DailyGoalMinutes { get; set; }
    public int TotalAnswered { get; set; }
    public int CorrectAnswered { get; set; }
}
