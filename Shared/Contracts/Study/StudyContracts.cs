using System.ComponentModel.DataAnnotations;
using Shared.Contracts.Common;
using Shared.Contracts.Deck;

namespace Shared.Contracts.Study;

/// <summary>
/// Response payload for GET /api/study/next
/// </summary>
public sealed class NextQuestionResponse
{
    public Question Question { get; set; } = default!;
}

/// <summary>
/// Query payload for GET /api/study/next?deckId=...
/// Why: enable DataAnnotations validation for query params (Guid.Empty, etc.)
/// </summary>
public sealed class GetNextQuestionRequest
{
    [NotEmptyGuid]
    public Guid DeckId { get; set; }
}

/// <summary>
/// Request payload for POST /api/study/answer
/// </summary>
public sealed class SubmitAnswerRequest
{
    [NotEmptyGuid]
    public Guid DeckId { get; set; }

    [NotEmptyGuid]
    public Guid QuestionId { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(32)]
    // Keep flexible: allow "A", "B", "C", "D" or future keys like "opt_1"
    [RegularExpression(@"^[A-Za-z0-9_\-]+$", ErrorMessage = "SelectedKey contains invalid characters.")]
    public string SelectedKey { get; set; } = string.Empty;
}

/// <summary>
/// Response payload for POST /api/study/answer
/// </summary>
public sealed class SubmitAnswerResponse
{
    public bool IsCorrect { get; set; }

    /// <summary>
    /// Return the correct key so UI can highlight the correct answer when user is wrong.
    /// </summary>
    public string? CorrectKey { get; set; }
}

/// <summary>
/// Response payload for GET /api/study/today
/// </summary>
public sealed class TodayProgressResponse
{
    public int DailyGoalMinutes { get; set; }
    public int TotalAnswered { get; set; }
    public int CorrectAnswered { get; set; }
}