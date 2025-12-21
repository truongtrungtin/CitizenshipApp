namespace Shared.Contracts.Me;

public sealed class MeSettingsResponse
{
    public string PreferredLanguage { get; set; } = "vi";
    public string FontScale { get; set; } = "L"; // M/L/XL (ví dụ)
}
