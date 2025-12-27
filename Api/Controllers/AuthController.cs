using Api.Auth;
using Api.Infrastructure;
using Domain.Entities.Users;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Auth;

namespace Api.Controllers;

/// <summary>
/// Auth endpoints (MVP): register / login.
///
/// Luồng đăng ký:
/// 1) Tạo AppUser (Identity)
/// 2) Tạo UserProfile (IsOnboarded=false)
/// 3) Tạo UserSettings (default elderly-first)
/// 4) Trả về JWT + IsOnboarded
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly JwtTokenService _jwt;
    private readonly AppDbContext _db;

    public AuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        JwtTokenService jwt,
        AppDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwt = jwt;
        _db = db;
    }

    /// <summary>
    /// POST /api/auth/register
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto req)
    {
        // Basic validation (MVP)
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        {
            return BadRequest("Username và Password là bắt buộc.");
        }

        var normalized = req.Username.Trim();

        // Tránh trùng username
        var existing = await _userManager.FindByNameAsync(normalized);
        if (existing is not null)
        {
            return Conflict("Tài khoản đã tồn tại.");
        }

        // Transaction (reusable helper):
        // - Tránh trạng thái "mồ côi": có AppUser nhưng thiếu Profile/Settings.
        // - Nếu Identity không enlist cùng transaction (trường hợp hiếm), vẫn có cleanup best-effort ở catch.

        // MVP: dùng Username cho cả email/phone
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = normalized,
            Email = normalized.Contains('@') ? normalized : null,
            PhoneNumber = normalized.Contains('@') ? null : normalized
        };

        try
        {
            return await _db.RunInTransactionAsync<ActionResult<AuthResponseDto>>(async () =>
            {
                // 1) Create identity user
                var create = await _userManager.CreateAsync(user, req.Password);
                if (!create.Succeeded)
                {
                    // Không commit transaction.
                    var msg = string.Join(" ", create.Errors.Select(e => e.Description));
                    return (commit: false, result: (ActionResult<AuthResponseDto>)BadRequest(msg));
                }

                // 2) Tạo Profile + Settings (default)
                var now = DateTime.UtcNow;
                var profile = new UserProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    IsOnboarded = false,
                    CreatedUtc = now,
                    UpdatedUtc = now
                };

                var settings = new UserSettings
                {
                    // Shared PK pattern: Settings.Id = Profile.Id
                    Id = profile.Id,
                    UserId = user.Id,
                    // Các default đã set trong entity
                    CreatedUtc = now,
                    UpdatedUtc = now
                };

                _db.UserProfiles.Add(profile);
                _db.UserSettings.Add(settings);
                await _db.SaveChangesAsync();

                // 3) Auto-login: trả về JWT
                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwt.CreateAccessToken(user, roles);

                // Commit khi toàn bộ bước thành công.
                return (commit: true, result: (ActionResult<AuthResponseDto>)Ok(new AuthResponseDto(token, IsOnboarded: false)));
            });
        }
        catch
        {
            // Compensating action:
            // Nếu vì lý do nào đó user đã được lưu ở ngoài transaction, ta cố gắng xóa user để tránh orphan.
            try
            {
                var existingUser = await _userManager.FindByNameAsync(normalized);
                if (existingUser is not null)
                {
                    await _userManager.DeleteAsync(existingUser);
                }
            }
            catch
            {
                // Nếu xóa fail, không nên che lỗi gốc ở đây.
                // (Có thể log sau nếu bạn thêm logging/telemetry.)
            }

            // Trả message trung lập (không lộ chi tiết nội bộ).
            return Problem("Register failed. Please try again.");
        }
    }

    /// <summary>
    /// POST /api/auth/login
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        {
            return BadRequest("Username và Password là bắt buộc.");
        }

        var normalized = req.Username.Trim();
        var user = await _userManager.FindByNameAsync(normalized);
        if (user is null)
        {
            return Unauthorized("Sai tài khoản hoặc mật khẩu.");
        }

        // Kiểm tra password
        var ok = await _signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: false);
        if (!ok.Succeeded)
        {
            return Unauthorized("Sai tài khoản hoặc mật khẩu.");
        }

        // Lấy IsOnboarded
        var profile = await _db.UserProfiles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == user.Id);

        var isOnboarded = profile?.IsOnboarded ?? false;

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwt.CreateAccessToken(user, roles);

        return Ok(new AuthResponseDto(token, isOnboarded));


    }
}
