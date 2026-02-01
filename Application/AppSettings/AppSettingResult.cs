using Shared.Contracts.AppSettings;

namespace Application.AppSettings;

/// <summary>
/// Result for AppSettings operations.
/// Why:
/// - Keep controllers thin while preserving error details.
/// </summary>
public sealed record AppSettingResult
{
    public bool Succeeded { get; init; }

    public AppSettingDto? Item { get; init; }

    public AppSettingFailureReason FailureReason { get; init; }

    public string? ErrorMessage { get; init; }

    public static AppSettingResult Success(AppSettingDto? item = null)
        => new()
        {
            Succeeded = true,
            Item = item,
            FailureReason = AppSettingFailureReason.None
        };

    public static AppSettingResult Fail(AppSettingFailureReason reason, string? message = null)
        => new()
        {
            Succeeded = false,
            FailureReason = reason,
            ErrorMessage = message
        };
}

/// <summary>
/// Failure categories for AppSettings operations.
/// </summary>
public enum AppSettingFailureReason
{
    None = 0,
    Conflict = 1,
    NotFound = 2,
    Failure = 3
}
