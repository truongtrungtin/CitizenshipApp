using System.ComponentModel.DataAnnotations;

namespace Shared.Contracts.AppSettings;

public sealed record AppSettingDto(int Id, string Key, string Value);

public sealed record CreateAppSettingRequest(
    [property: Required]
    [property: MinLength(1)]
    [property: MaxLength(100)]
    // Supports keys like "Jwt:Key", "Cors:AllowedOrigins", "Proxy:Enabled"
    [property: RegularExpression(@"^[A-Za-z0-9_\-.:]+$", ErrorMessage = "Key has invalid characters.")]
    string Key,

    [property: MaxLength(4000)]
    string Value
);

public sealed record UpdateAppSettingRequest(
    [property: Required]
    [property: MinLength(1)]
    [property: MaxLength(100)]
    [property: RegularExpression(@"^[A-Za-z0-9_\-.:]+$", ErrorMessage = "Key has invalid characters.")]
    string Key,

    [property: MaxLength(4000)]
    string Value
);