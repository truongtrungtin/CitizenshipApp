using System.Diagnostics;

namespace Api.Infrastructure.Middleware;

/// <summary>
/// Adds Correlation ID to each request/response and writes minimal request logs.
/// - Reads from X-Correlation-ID if provided
/// - Otherwise generates a new one
/// - Stores it in HttpContext.Items for downstream components (ProblemDetails, etc.)
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    private const string ItemKey = "CorrelationId";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1) Resolve correlation id (client provided -> reuse, else generate)
        string correlationId =
            context.Request.Headers.TryGetValue(HeaderName, out var values) &&
            !string.IsNullOrWhiteSpace(values.FirstOrDefault())
                ? values.First()!
                : Guid.NewGuid().ToString("N");

        // 2) Store it so ProblemDetails / other code can reuse it
        context.Items[ItemKey] = correlationId;

        // 3) Always return it to caller
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        // 4) Minimal request logging (no body, no headers dump)
        //    Skip noisy endpoints if you want (health checks).
        bool skipLogging =
            context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/swagger");

        var sw = Stopwatch.StartNew();

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            if (!skipLogging)
            {
                _logger.LogInformation("HTTP {Method} {Path} started",
                    context.Request.Method,
                    context.Request.Path.Value);
            }

            await _next(context);

            sw.Stop();

            if (!skipLogging)
            {
                _logger.LogInformation("HTTP {Method} {Path} finished {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.Response.StatusCode,
                    sw.ElapsedMilliseconds);
            }
        }
    }

    /// <summary>Get correlation id from HttpContext (if middleware executed).</summary>
    public static string? TryGet(HttpContext context)
        => context.Items.TryGetValue(ItemKey, out var v) ? v?.ToString() : null;
}
