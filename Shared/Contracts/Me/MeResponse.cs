namespace Shared.Contracts.Me;

public sealed class MeResponse
{
    public string Email { get; set; } = string.Empty;
    public bool IsOnboarded { get; set; }
}
