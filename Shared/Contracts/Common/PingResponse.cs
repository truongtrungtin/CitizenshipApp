namespace Shared.Contracts.Ping;

/// <summary>
/// Response returned by GET /api/ping.
/// </summary>
public sealed record PingResponse(string Message);
