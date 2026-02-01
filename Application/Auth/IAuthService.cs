using Shared.Contracts.Auth;

namespace Application.Auth;

/// <summary>
/// Application contract for auth workflows.
/// Controllers depend on this interface to stay thin.
/// </summary>
public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest req, CancellationToken ct);

    Task<AuthResult> LoginAsync(LoginRequest req, CancellationToken ct);
}
