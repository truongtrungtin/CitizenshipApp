namespace Shared.Dtos.Auth;

/// <summary>
/// Request DTO cho /auth/login.
/// </summary>
public sealed record LoginRequestDto(
    string Username,
    string Password);
