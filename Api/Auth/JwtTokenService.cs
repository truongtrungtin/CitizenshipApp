using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api.Auth;

/// <summary>
/// Service tạo JWT access token cho người dùng.
///
/// MVP scope:
/// - Tạo access token (JWT)
/// - Chưa triển khai refresh token (có thể thêm sau)
/// </summary>
public sealed class JwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        // Lấy config đã bind từ appsettings (Jwt section)
        _options = options.Value;
    }

    /// <summary>
    /// Tạo access token cho user.
    /// </summary>
    /// <param name="user">Identity user.</param>
    /// <param name="roles">Danh sách role đã gán cho user (có thể rỗng/null).</param>
    /// <returns>JWT string.</returns>
    public string CreateAccessToken(AppUser user, IEnumerable<string>? roles)
    {
        // -----------------------------
        // Defensive checks (để debug dễ)
        // -----------------------------
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        // Nếu roles null, chuyển sang collection rỗng để foreach không bị crash
        roles ??= Array.Empty<string>();

        // Secret key tối thiểu nên đủ dài (khuyến nghị >= 32 ký tự)
        if (string.IsNullOrWhiteSpace(_options.Key))
            throw new InvalidOperationException("JWT Key is missing. Please set Jwt:Key in configuration.");

        // -----------------------------
        // Build claims
        // -----------------------------
        // Claims tiêu chuẩn:
        // - sub: định danh user (Guid)
        // - jti: id của token (để trace/blacklist nếu cần về sau)
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };

        // Email claim (nếu có)
        if (!string.IsNullOrWhiteSpace(user.Email))
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));

        // UniqueName claim (username)
        if (!string.IsNullOrWhiteSpace(user.UserName))
            claims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName));

        // Role claims (để [Authorize(Roles = "Admin")] hoạt động)
        foreach (var role in roles)
        {
            // Skip role rỗng để tránh claim bẩn
            if (string.IsNullOrWhiteSpace(role))
                continue;

            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // -----------------------------
        // Sign token (HMAC-SHA256)
        // -----------------------------
        var keyBytes = Encoding.UTF8.GetBytes(_options.Key);
        var signingKey = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        // Thời hạn token
        var expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        // -----------------------------
        // Create JWT
        // -----------------------------
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        // Serialize token => string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
