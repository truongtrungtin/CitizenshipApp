namespace Shared.Dtos.Auth;

/// <summary>
/// Request DTO cho /auth/register.
///
/// Elderly-first:
/// - Cho phép user dùng Email hoặc Phone làm username.
/// - MVP: chúng ta dùng UserName là email/phone string.
/// </summary>
public sealed record RegisterRequestDto(
    string Username,
    string Password);
