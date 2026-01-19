namespace Ui.Blazor.Services;

/// <summary>
/// Represents an API error response in a UI-friendly way.
/// Contains both general problem info and field-level validation errors.
/// </summary>
public sealed class ApiException : Exception
{
    public int StatusCode { get; }
    public string? Title { get; }
    public string? Detail { get; }
    public string? TraceId { get; }

    /// <summary>
    /// Field-level errors: "Email" -> ["Required", "..."]
    /// Keys are case-insensitive by design.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ApiException(
        int statusCode,
        string message,
        string? title = null,
        string? detail = null,
        string? traceId = null,
        IReadOnlyDictionary<string, string[]>? errors = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Title = title;
        Detail = detail;
        TraceId = traceId;
        Errors = errors ?? new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    }
}
