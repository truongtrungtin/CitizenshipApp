namespace Shared.Dtos.Auth;

/// <summary>
/// Response DTO trả về sau khi login/register.
/// </summary>
public sealed record AuthResponseDto(
    string AccessToken,
    bool IsOnboarded);
