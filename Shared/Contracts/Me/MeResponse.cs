namespace Shared.Contracts.Me;

public sealed class MeResponse
{
    public string Email { get; set; } = string.Empty;
    public bool IsOnboarded { get; set; }
}

/// <summary>
///     Response payload for GET /api/me/profile
/// </summary>
public sealed class MeProfileResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsOnboarded { get; set; }
}
