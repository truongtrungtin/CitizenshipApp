using Shared.Contracts.Auth;

namespace Application.Auth;

/// <summary>
/// Represents the outcome of an auth operation.
/// Why:
/// - Keep controllers thin by returning a rich, structured result.
/// - Preserve API behavior without leaking infrastructure details.
/// </summary>
public sealed record AuthResult
{
    public bool Succeeded { get; init; }

    public AuthResponse? Response { get; init; }

    public AuthFailureReason FailureReason { get; init; }

    public string? ErrorMessage { get; init; }

    public static AuthResult Success(AuthResponse response)
        => new()
        {
            Succeeded = true,
            Response = response,
            FailureReason = AuthFailureReason.None
        };

    public static AuthResult Fail(AuthFailureReason reason, string? message = null)
        => new()
        {
            Succeeded = false,
            FailureReason = reason,
            ErrorMessage = message
        };
}

/// <summary>
/// Error categories for mapping to HTTP responses.
/// </summary>
public enum AuthFailureReason
{
    None = 0,
    BadRequest = 1,
    Unauthorized = 2,
    Conflict = 3,
    NotFound = 4,
    Failure = 5
}
