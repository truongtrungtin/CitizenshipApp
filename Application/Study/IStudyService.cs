using Shared.Contracts.Study;

namespace Application.Study;

/// <summary>
/// Application contract for study flow.
/// Controllers depend on this interface so they don't depend on EF directly.
/// Implementation lives in Infrastructure.
/// </summary>
public interface IStudyService
{
    /// <summary>
    /// Get the next question for the given deck.
    /// Returns null if the deck has no questions.
    /// </summary>
    Task<NextQuestionResponse?> GetNextQuestionAsync(Guid userId, GetNextQuestionRequest req, CancellationToken ct);

    /// <summary>
    /// Submit an answer and record a StudyEvent.
    /// Returns null if the question is not found (or not in the deck).
    /// </summary>
    Task<SubmitAnswerResponse?> SubmitAnswerAsync(Guid userId, SubmitAnswerRequest req, CancellationToken ct);

    /// <summary>
    /// Get today's progress for the user.
    /// </summary>
    Task<TodayProgressResponse> GetTodayProgressAsync(Guid userId, CancellationToken ct);
}
