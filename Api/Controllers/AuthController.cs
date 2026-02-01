using Application.Auth;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using Shared.Contracts.Auth;

namespace Api.Controllers;

/// <summary>
///     Auth endpoints (MVP): register / login.
///     Luồng đăng ký:
///     1) Tạo AppUser (Identity)
///     2) Tạo UserProfile (IsOnboarded=false)
///     3) Tạo UserSettings (default elderly-first)
///     4) Trả về JWT + IsOnboarded
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    /// <summary>
    ///     POST /api/auth/register
    /// </summary>
    [HttpPost("register")]
    [EnableRateLimiting("auth-register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        AuthResult result = await auth.RegisterAsync(req, ct);
        return MapAuthResult(result, "Register failed. Please try again.");
    }

    /// <summary>
    ///     POST /api/auth/login
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("auth-login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        AuthResult result = await auth.LoginAsync(req, ct);
        return MapAuthResult(result, "Login failed. Please try again.");
    }

    private ActionResult<AuthResponse> MapAuthResult(AuthResult result, string defaultError)
    {
        if (result.Succeeded && result.Response is not null)
        {
            return Ok(result.Response);
        }

        return result.FailureReason switch
        {
            AuthFailureReason.Conflict => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: result.ErrorMessage),
            AuthFailureReason.BadRequest => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                detail: result.ErrorMessage),
            AuthFailureReason.Unauthorized => Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: result.ErrorMessage),
            AuthFailureReason.NotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: result.ErrorMessage),
            _ => Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Server Error",
                detail: defaultError)
        };
    }
}
