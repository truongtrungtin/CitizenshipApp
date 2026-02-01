using System.ComponentModel.DataAnnotations;

namespace Shared.Contracts.AppSettings;

public sealed record AppSettingDto(int Id, string Key, string Value);

public sealed record CreateAppSettingRequest(
    [param: Required]
    [param: MinLength(1)]
    [param: MaxLength(100)]
    // Supports keys like "Jwt:Key", "Cors:AllowedOrigins", "Proxy:Enabled"
    [param: RegularExpression(@"^[A-Za-z0-9_\-.:]+$", ErrorMessage = "Key has invalid characters.")]
    string Key,

    [param: MaxLength(4000)]
    string Value
);

public sealed record UpdateAppSettingRequest(
    [param: Required]
    [param: MinLength(1)]
    [param: MaxLength(100)]
    [param: RegularExpression(@"^[A-Za-z0-9_\-.:]+$", ErrorMessage = "Key has invalid characters.")]
    string Key,

    [param: MaxLength(4000)]
    string Value
);
