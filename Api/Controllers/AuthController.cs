using Api.Auth;
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
[Route("auth")]
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
    /// POST /auth/register
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

        // MVP: dùng Username cho cả email/phone
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = normalized,
            Email = normalized.Contains('@') ? normalized : null,
            PhoneNumber = normalized.Contains('@') ? null : normalized
        };

        var create = await _userManager.CreateAsync(user, req.Password);
        if (!create.Succeeded)
        {
            // Trả về message đơn giản, dễ hiểu cho user.
            var msg = string.Join(" ", create.Errors.Select(e => e.Description));
            return BadRequest(msg);
        }

        // Tạo Profile + Settings (default)
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            IsOnboarded = false,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };

        var settings = new UserSettings
        {
            // Shared PK pattern: Settings.Id = Profile.Id
            Id = profile.Id,
            UserId = user.Id,
            // Các default đã set trong entity
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };

        _db.UserProfiles.Add(profile);
        _db.UserSettings.Add(settings);
        await _db.SaveChangesAsync();

        // Auto-login: trả về JWT
        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwt.CreateAccessToken(user, roles);

        return Ok(new AuthResponseDto(token, IsOnboarded: false));
    }

    /// <summary>
    /// POST /auth/login
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
