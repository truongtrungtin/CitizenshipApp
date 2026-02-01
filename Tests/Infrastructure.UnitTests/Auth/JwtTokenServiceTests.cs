using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Infrastructure.Auth;

using Infrastructure.Identity;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.UnitTests.Auth;

/// <summary>
/// Unit tests for <see cref="JwtTokenService"/> (BL-021).
/// We verify that the service creates JWT access tokens with:
/// - correct issuer/audience
/// - required claims (sub, nameid, jti)
/// - optional claims when data exists (email, unique_name)
/// - role claims
/// And we validate the token signature and lifetime using TokenValidationParameters.
/// </summary>
public sealed class JwtTokenServiceTests
{
    /// <summary>
    /// Creates a JwtTokenService with test options.
    /// </summary>
    private static JwtTokenService CreateService(string key, int accessTokenMinutes = 15)
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            Key = key,
            AccessTokenMinutes = accessTokenMinutes
        });

        return new JwtTokenService(options);
    }

    /// <summary>
    /// Creates token validation parameters aligned with the JwtOptions we set in tests.
    /// ClockSkew is set to zero to avoid flakiness in expiration tests.
    /// </summary>
    private static TokenValidationParameters CreateValidationParams(string key)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = "test-issuer",
            ValidAudience = "test-audience",

            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

            ClockSkew = TimeSpan.Zero
        };
    }

    /// <summary>
    /// Builds a minimal AppUser for token generation.
    /// Note: We set Id/Email/UserName because JwtTokenService emits claims based on these fields.
    /// </summary>
    private static AppUser CreateUser(Guid id, string? email = "user@test.local", string? userName = "testuser")
    {
        return new AppUser
        {
            Id = id,
            Email = email,
            UserName = userName
        };
    }

    [Fact]
    public void CreateAccessToken_ShouldContainRequiredClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUser(userId);
        var service = CreateService("super-secret-test-key-1234567890-super-secret");

        // Act
        var token = service.CreateAccessToken(user, roles: Array.Empty<string>());
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        // Required standard claims from your implementation:
        // - sub: userId
        // - nameid: userId (ClaimTypes.NameIdentifier)
        // - jti: random id
        Assert.Equal(userId.ToString(), jwt.Subject);

        Assert.Contains(jwt.Claims, c =>
            c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());

        Assert.Contains(jwt.Claims, c =>
            c.Type == JwtRegisteredClaimNames.Jti && !string.IsNullOrWhiteSpace(c.Value));

        // Issuer + audience
        Assert.Equal("test-issuer", jwt.Issuer);
        Assert.Contains("test-audience", jwt.Audiences);
    }

    [Fact]
    public void CreateAccessToken_ShouldIncludeOptionalClaims_WhenUserHasEmailAndUserName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUser(userId, email: "a@b.com", userName: "tin");
        var service = CreateService("super-secret-test-key-1234567890-super-secret");

        // Act
        var token = service.CreateAccessToken(user, roles: Array.Empty<string>());
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "a@b.com");
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.UniqueName && c.Value == "tin");
    }

    [Fact]
    public void CreateAccessToken_ShouldIncludeRoleClaims()
    {
        // Arrange
        var user = CreateUser(Guid.NewGuid());
        var service = CreateService("super-secret-test-key-1234567890-super-secret");

        var roles = new[] { "Admin", "User" };

        // Act
        var token = service.CreateAccessToken(user, roles);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "User");
    }

    [Fact]
    public void CreatedToken_ShouldValidate_WithCorrectSigningKey()
    {
        // Arrange
        var key = "super-secret-test-key-1234567890-super-secret";
        var user = CreateUser(Guid.NewGuid());
        var service = CreateService(key);

        var token = service.CreateAccessToken(user, roles: Array.Empty<string>());
        var validationParams = CreateValidationParams(key);

        // Act
        var handler = new JwtSecurityTokenHandler();
        handler.ValidateToken(token, validationParams, out _);

        // Assert
        // If no exception was thrown, signature + issuer + audience + lifetime are valid.
        Assert.True(true);
    }

    [Fact]
    public void TokenValidation_ShouldFail_WithWrongSigningKey()
    {
        // Arrange
        var correctKey = "correct-key-1234567890-correct-key";
        var wrongKey = "wrong-key-1234567890-wrong-key__";

        var user = CreateUser(Guid.NewGuid());
        var service = CreateService(correctKey);

        var token = service.CreateAccessToken(user, roles: Array.Empty<string>());
        var validationParams = CreateValidationParams(wrongKey);

        // Act + Assert
        var ex = Assert.ThrowsAny<SecurityTokenException>(() =>
        {
            new JwtSecurityTokenHandler().ValidateToken(token, validationParams, out _);
        });

        // Accept either "invalid signature" or "signature key not found"
        Assert.True(
            ex is SecurityTokenInvalidSignatureException ||
            ex is SecurityTokenSignatureKeyNotFoundException);
    }

    [Fact]
    public void TokenValidation_ShouldFail_WhenExpired()
    {
        // Arrange
        // Create token with a very short lifetime (0 minutes).
        var key = "super-secret-test-key-1234567890-super-secret";
        var user = CreateUser(Guid.NewGuid());

        // Setting minutes to 0 makes expiration essentially immediate.
        var service = CreateService(key, accessTokenMinutes: 0);

        var token = service.CreateAccessToken(user, roles: Array.Empty<string>());
        var validationParams = CreateValidationParams(key);

        // Act + Assert
        Assert.Throws<SecurityTokenExpiredException>(() =>
        {
            new JwtSecurityTokenHandler().ValidateToken(token, validationParams, out _);
        });
    }
}
