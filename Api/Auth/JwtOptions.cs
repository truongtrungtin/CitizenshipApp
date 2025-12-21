namespace Api.Auth;

/// <summary>
/// Strongly-typed config cho JWT.
///
/// Nguồn cấu hình:
/// - Api/appsettings.json
/// - Api/appsettings.Development.json
///
/// Lưu ý bảo mật:
/// - Key nên để trong Secret Manager/ENV ở môi trường thật.
/// - Dev có thể để trong appsettings.Development.json.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Section name trong configuration.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Issuer của token (ví dụ: "CitizenshipApp").
    /// </summary>
    public string Issuer { get; set; } = "CitizenshipApp";

    /// <summary>
    /// Audience của token (ví dụ: "CitizenshipApp.Ui").
    /// </summary>
    public string Audience { get; set; } = "CitizenshipApp.Ui";

    /// <summary>
    /// Secret key để ký token (HMAC-SHA256).
    ///
    /// IMPORTANT: tối thiểu 32 ký tự để đủ entropy.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Thời hạn token (phút).
    /// </summary>
    public int AccessTokenMinutes { get; set; } = 60;
}
