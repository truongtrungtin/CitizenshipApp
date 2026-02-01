namespace Infrastructure.Auth;

/// <summary>
/// Strongly-typed config for JWT.
/// Why:
/// - Centralize token configuration outside of Api project.
/// - Keep Auth services in Infrastructure.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Section name in configuration.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Token issuer (e.g. "CitizenshipApp").
    /// </summary>
    public string Issuer { get; set; } = "CitizenshipApp";

    /// <summary>
    /// Token audience (e.g. "CitizenshipApp.Ui").
    /// </summary>
    public string Audience { get; set; } = "CitizenshipApp.Ui";

    /// <summary>
    /// Secret key used for HMAC-SHA256 signing.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Access token lifetime (minutes).
    /// </summary>
    public int AccessTokenMinutes { get; set; } = 60;
}
