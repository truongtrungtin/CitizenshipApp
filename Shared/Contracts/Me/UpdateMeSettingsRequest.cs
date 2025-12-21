using System.ComponentModel.DataAnnotations;

namespace Shared.Contracts.Me;

public sealed class UpdateMeSettingsRequest
{
    [Required]
    public string PreferredLanguage { get; set; } = "vi";

    [Required]
    public string FontScale { get; set; } = "L";
}
