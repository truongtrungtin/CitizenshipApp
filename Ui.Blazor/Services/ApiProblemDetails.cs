using System.Text.Json.Serialization;

namespace Ui.Blazor.Services;

/// <summary>
/// Minimal RFC7807 ProblemDetails model for UI.
/// Matches ASP.NET Core ProblemDetails JSON fields.
/// </summary>
public class ApiProblemDetails
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    [JsonPropertyName("instance")]
    public string? Instance { get; set; }

    /// <summary>
    /// Extensions can include traceId or other extra info.
    /// We only care about traceId for debugging/correlation.
    /// </summary>
    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }
}

/// <summary>
/// ValidationProblemDetails adds "errors" dictionary: field -> array of messages.
/// </summary>
public sealed class ApiValidationProblemDetails : ApiProblemDetails
{
    [JsonPropertyName("errors")]
    public Dictionary<string, string[]> Errors { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
