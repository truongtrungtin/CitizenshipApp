using System.ComponentModel.DataAnnotations;

using Shared.Contracts.Common;

namespace Shared.Contracts.Auth;

public sealed class LoginRequest
{
    /// <summary>
    /// Username can be email OR phone.
    /// Keep property name as Email to avoid breaking current UI/JSON contract.
    /// </summary>
    [Required]
    [EmailOrPhone]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}
