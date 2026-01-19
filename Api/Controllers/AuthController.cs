using Api.Auth;
using Api.Infrastructure;

using Domain.Entities.Users;

using Infrastructure.Identity;
using Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Shared.Contracts.Auth;

using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

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
public sealed class AuthController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    JwtTokenService jwt,
    AppDbContext db)
    : ControllerBase
{
    /// <summary>
    ///     POST /api/auth/register
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
    {

        string normalized = req.Email.Trim();

        // Tránh trùng username
        AppUser? existing = await userManager.FindByNameAsync(normalized);
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
            return await db.RunInTransactionAsync(async () =>
            {
                // 1) Create identity user
                IdentityResult create = await userManager.CreateAsync(user, req.Password);
                if (!create.Succeeded)
                {
                    // Không commit transaction.
                    string msg = string.Join(" ", create.Errors.Select(e => e.Description));
                    return (commit: false, result: BadRequest(msg));
                }

                // 2) Tạo Profile + Settings (default)
                DateTime now = DateTime.UtcNow;
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

                db.UserProfiles.Add(profile);
                db.UserSettings.Add(settings);
                await db.SaveChangesAsync();

                // 3) Auto-login: trả về JWT
                IList<string> roles = await userManager.GetRolesAsync(user);
                string token = jwt.CreateAccessToken(user, roles);

                // Commit khi toàn bộ bước thành công.
                return (commit: true, result: (ActionResult<AuthResponse>)Ok(new AuthResponse
                {
                    AccessToken = token,
                    UserId = user.Id,
                    IsOnboarded = false
                }));
            });
        }
        catch
        {
            // Compensating action:
            // Nếu vì lý do nào đó user đã được lưu ở ngoài transaction, ta cố gắng xóa user để tránh orphan.
            try
            {
                AppUser? existingUser = await userManager.FindByNameAsync(normalized);
                if (existingUser is not null)
                {
                    await userManager.DeleteAsync(existingUser);
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
    ///     POST /api/auth/login
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        string normalized = req.Email.Trim();
        AppUser? user = await userManager.FindByNameAsync(normalized);
        if (user is null)
        {
            return Unauthorized("Sai tài khoản hoặc mật khẩu.");
        }

        // Kiểm tra password
        SignInResult ok = await signInManager.CheckPasswordSignInAsync(user, req.Password, false);
        if (!ok.Succeeded)
        {
            return Unauthorized("Sai tài khoản hoặc mật khẩu.");
        }

        // Lấy IsOnboarded
        UserProfile? profile = await db.UserProfiles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == user.Id);

        bool isOnboarded = profile?.IsOnboarded ?? false;

        IList<string> roles = await userManager.GetRolesAsync(user);
        string token = jwt.CreateAccessToken(user, roles);

        return Ok(new AuthResponse
        {
            AccessToken = token,
            UserId = user.Id,
            IsOnboarded = isOnboarded
        });
    }
}
