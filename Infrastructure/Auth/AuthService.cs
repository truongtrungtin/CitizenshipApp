using Application.Auth;

using Domain.Entities.Users;

using Infrastructure.Identity;
using Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Shared.Contracts.Auth;

using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Infrastructure.Auth;

/// <summary>
/// Infrastructure implementation of <see cref="IAuthService" />.
/// Why:
/// - Keeps controllers thin.
/// - Centralizes Identity + EF Core logic in Infrastructure.
/// </summary>
public sealed class AuthService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    JwtTokenService jwt,
    AppDbContext db,
    ILogger<AuthService> logger,
    IHostEnvironment env)
    : IAuthService
{
    public async Task<AuthResult> RegisterAsync(RegisterRequest req, CancellationToken ct)
    {
        string normalized = req.Email.Trim();

        // Why: Prevent duplicate usernames/emails early.
        AppUser? existing = await userManager.FindByNameAsync(normalized);
        if (existing is not null)
        {
            return AuthResult.Fail(AuthFailureReason.Conflict, "Tài khoản đã tồn tại.");
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = normalized,
            Email = normalized.Contains('@') ? normalized : null,
            PhoneNumber = normalized.Contains('@') ? null : normalized
        };

        try
        {
            // 1) Create identity user
            IdentityResult create = await userManager.CreateAsync(user, req.Password);
            if (!create.Succeeded)
            {
                string msg = string.Join(" ", create.Errors.Select(e => e.Description));
                return AuthResult.Fail(AuthFailureReason.BadRequest, msg);
            }

            // 2) Create Profile + Settings
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
                CreatedUtc = now,
                UpdatedUtc = now
            };

            db.UserProfiles.Add(profile);
            db.UserSettings.Add(settings);
            await db.SaveChangesAsync(ct);

            IList<string> roles = await userManager.GetRolesAsync(user);
            string token = jwt.CreateAccessToken(user, roles);

            return AuthResult.Success(new AuthResponse
            {
                AccessToken = token,
                UserId = user.Id,
                IsOnboarded = false
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Register failed for {User}.", normalized);

            // Best-effort cleanup if identity user was created but related rows failed.
            try
            {
                AppUser? created = await userManager.FindByNameAsync(normalized);
                if (created is not null)
                {
                    await userManager.DeleteAsync(created);
                }
            }
            catch
            {
                // Avoid masking the root exception.
            }

            string msg = env.IsDevelopment()
                ? ex.GetBaseException().Message
                : "Register failed. Please try again.";

            return AuthResult.Fail(AuthFailureReason.Failure, msg);
        }
    }

    public async Task<AuthResult> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        string normalized = req.Email.Trim();
        AppUser? user = await userManager.FindByNameAsync(normalized);
        if (user is null)
        {
            return AuthResult.Fail(AuthFailureReason.Unauthorized, "Sai tài khoản hoặc mật khẩu.");
        }

        SignInResult ok = await signInManager.CheckPasswordSignInAsync(user, req.Password, false);
        if (!ok.Succeeded)
        {
            return AuthResult.Fail(AuthFailureReason.Unauthorized, "Sai tài khoản hoặc mật khẩu.");
        }

        UserProfile? profile = await db.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == user.Id, ct);

        bool isOnboarded = profile?.IsOnboarded ?? false;

        IList<string> roles = await userManager.GetRolesAsync(user);
        string token = jwt.CreateAccessToken(user, roles);

        return AuthResult.Success(new AuthResponse
        {
            AccessToken = token,
            UserId = user.Id,
            IsOnboarded = isOnboarded
        });
    }
}
