using Shared.Contracts.Deck;

namespace Shared.Contracts.Study;

/// <summary>
/// Request: user chọn option key nào cho câu hỏi.
/// </summary>
public sealed record SubmitAnswerRequestDto(Guid DeckId, Guid QuestionId, string SelectedKey);

/// <summary>
/// Response: đúng/sai + đáp án đúng (để UI show feedback).
/// </summary>
public sealed record SubmitAnswerResponseDto(bool IsCorrect, string CorrectKey);

/// <summary>
/// Progress hôm nay (MVP).
/// </summary>
public sealed record TodayProgressDto(int DailyGoal, int TotalAnswered, int CorrectAnswered);

/// <summary>
/// Question trả về cho UI (đã có QuestionDto trong Shared).
/// </summary>
public sealed record NextQuestionResponseDto(Question Question);
