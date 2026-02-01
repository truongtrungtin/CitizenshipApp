using Api.Infrastructure;

using Application.Me;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shared.Contracts.Me;

namespace Api.Controllers;

/// <summary>
///     Các endpoint "me" dành cho user đã đăng nhập.
///     Những endpoint này dùng để:
///     - Lấy trạng thái onboarding (IsOnboarded)
///     - Đọc/ghi UserSettings
///     NOTE: Mọi endpoint trong controller này đều require JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class MeController(IMeService me) : ApiControllerBase
{
    /// <summary>
    ///     GET /api/Me/profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<ActionResult<MeProfileResponse>> GetProfile(CancellationToken ct)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Missing or invalid access token.");
        }

        MeProfileResponse? profile = await me.GetProfileAsync(userId, ct);
        if (profile is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: "User profile not found.");
        }

        return Ok(profile);
    }


    /// <summary>
    ///     PUT /api/me/onboarding/complete
    ///     Chỉ đánh dấu hoàn tất onboarding.
    ///     (Settings đã được cập nhật qua /me/settings/full.)
    /// </summary>
    [HttpPut("onboarding/complete")]
    public async Task<IActionResult> CompleteOnboarding(CancellationToken ct)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Missing or invalid access token.");
        }

        bool updated = await me.CompleteOnboardingAsync(userId, ct);
        return updated
            ? NoContent()
            : Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: "User profile not found.");
    }


    [HttpGet("settings/full")]
    public async Task<ActionResult<UserSettingContracts>> GetSettingsFull(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Missing or invalid access token.");
        }

        UserSettingContracts settings = await me.GetSettingsFullAsync(userId, ct);
        return Ok(settings);
    }

    [HttpPut("settings/full")]
    public async Task<IActionResult> UpdateSettingsFull([FromBody] UserSettingContracts req, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Missing or invalid access token.");

        bool updated = await me.UpdateSettingsFullAsync(userId, req, ct);
        return updated
            ? NoContent()
            : Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: "User profile not found.");
    }
}
