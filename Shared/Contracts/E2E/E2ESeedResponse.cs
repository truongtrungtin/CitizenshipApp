namespace Shared.Contracts.E2E;

public sealed class E2ESeedResponse
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid DeckId { get; set; }
    public string DeckCode { get; set; } = string.Empty;
    public string DeckName { get; set; } = string.Empty;
}
