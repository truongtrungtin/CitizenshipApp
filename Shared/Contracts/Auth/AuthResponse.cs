namespace Shared.Contracts.Auth;

/// <summary>
/// Response payload for POST /api/auth/register and POST /api/auth/login
/// </summary>
public sealed class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public bool IsOnboarded { get; set; }
}
