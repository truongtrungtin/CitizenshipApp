using System.Net.Http.Json;
using System.Text.Json;

using Shared.Contracts.Auth;
using Shared.Contracts.Deck;
using Shared.Contracts.Me;
using Shared.Contracts.Study;

namespace Ui.Blazor.Services;

/// <summary>
/// Typed client for API endpoints used by the Blazor UI.
/// Authorization is added by <see cref="AuthHeaderHandler"/>.
/// 
/// Design goals:
/// - Never call EnsureSuccessStatusCode() because it throws HttpRequestException
///   and loses RFC7807 ProblemDetails details.
/// - For every non-success response, parse application/problem+json and throw ApiException.
/// - Keep JSON options consistent across read/write.
/// </summary>
public sealed class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        // Helps if server returns different casing (camelCase vs PascalCase)
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;

    public ApiClient(HttpClient http) => _http = http;

    /// <summary>
    /// Returns true when the response is RFC7807 ProblemDetails JSON.
    /// </summary>
    private static bool IsProblemJson(HttpResponseMessage response)
    {
        // Many servers return "application/problem+json; charset=utf-8"
        var contentType = response.Content.Headers.ContentType?.MediaType;
        return string.Equals(contentType, "application/problem+json", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Builds a rich ApiException by reading and parsing the error response body.
    /// Supports:
    /// - ValidationProblemDetails (has "errors")
    /// - ProblemDetails (title/detail/status/traceId)
    /// - Fallback: raw body as detail
    /// </summary>
    private static async Task<ApiException> CreateApiExceptionAsync(HttpResponseMessage response)
    {
        var status = (int)response.StatusCode;

        // Default fallback message.
        string message = $"Request failed with status code {status}.";
        string? body = null;

        try
        {
            body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
        catch
        {
            // Ignore body read errors; use fallback message.
        }

        if (!string.IsNullOrWhiteSpace(body))
        {
            // If server returned ProblemDetails, parse it.
            if (IsProblemJson(response))
            {
                // 1) Try ValidationProblemDetails first (has "errors").
                try
                {
                    var validation = JsonSerializer.Deserialize<ApiValidationProblemDetails>(body, JsonOptions);
                    if (validation is not null && validation.Errors.Count > 0)
                    {
                        return new ApiException(
                            statusCode: status,
                            message: validation.Title ?? message,
                            title: validation.Title,
                            detail: validation.Detail,
                            traceId: validation.TraceId,
                            errors: validation.Errors
                        );
                    }
                }
                catch
                {
                    // If it fails, fall back to ProblemDetails parse below.
                }

                // 2) Try normal ProblemDetails.
                try
                {
                    var problem = JsonSerializer.Deserialize<ApiProblemDetails>(body, JsonOptions);
                    if (problem is not null)
                    {
                        return new ApiException(
                            statusCode: status,
                            message: problem.Title ?? message,
                            title: problem.Title,
                            detail: problem.Detail,
                            traceId: problem.TraceId
                        );
                    }
                }
                catch
                {
                    // If parsing fails, fall back to raw body below.
                }
            }

            // Not problem+json or parse failed: keep raw body as detail (trim to avoid huge output).
            var trimmed = body.Length > 1000 ? body[..1000] + "..." : body;
            return new ApiException(status, message, detail: trimmed);
        }

        return new ApiException(status, message);
    }

    /// <summary>
    /// Reads JSON body from a successful response.
    /// Throws if body is empty (unexpected for our API).
    /// </summary>
    private static async Task<T> ReadSuccessJsonAsync<T>(HttpResponseMessage response, string endpointHint, CancellationToken ct)
    {
        var payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct).ConfigureAwait(false);
        return payload ?? throw new InvalidOperationException($"{endpointHint} returned empty body.");
    }

    // -------------------------
    // Auth
    // -------------------------

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("/api/Auth/register", request, JsonOptions, ct);

        if (!response.IsSuccessStatusCode)
            throw await CreateApiExceptionAsync(response);

        return await ReadSuccessJsonAsync<AuthResponse>(response, "POST /api/Auth/register", ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("/api/Auth/login", request, JsonOptions, ct);

        if (!response.IsSuccessStatusCode)
            throw await CreateApiExceptionAsync(response);

        return await ReadSuccessJsonAsync<AuthResponse>(response, "POST /api/Auth/login", ct);
    }

    // -------------------------
    // Me / Settings
    // -------------------------

    public async Task<MeSettingsResponse> GetMeSettingsAsync(CancellationToken ct = default)
    {
        using var response = await _http.GetAsync("/api/Me/settings", ct);

        if (!response.IsSuccessStatusCode)
            throw await CreateApiExceptionAsync(response);

        return await ReadSuccessJsonAsync<MeSettingsResponse>(response, "GET /api/me/settings", ct);
    }

    public async Task<MeSettingsResponse> UpdateMeSettingsAsync(UpdateMeSettingsRequest request, CancellationToken ct = default)
    {
        using var response = await _http.PutAsJsonAsync("/api/Me/settings", request, JsonOptions, ct);

        if (!response.IsSuccessStatusCode)
            throw await CreateApiExceptionAsync(response);

        return await ReadSuccessJsonAsync<MeSettingsResponse>(response, "PUT /api/me/settings", ct);
    }

    public async Task CompleteOnboardingAsync(CancellationToken ct = default)
    {
        using var response = await _http.PutAsync("/api/Me/onboarding/complete", content: null, ct);

        if (!response.IsSuccessStatusCode)
            throw await CreateApiExceptionAsync(response);

        // No body expected.
    }

    public async Task<UserSettingContracts> GetMeSettingsFullAsync(CancellationToken ct = default)
    {
        var res = await _http.GetAsync("api/Me/settings/full", ct);
        if (!res.IsSuccessStatusCode)
            throw await CreateApiExceptionAsync(res);

        var payload = await res.Content.ReadFromJsonAsync<UserSettingContracts>(cancellationToken: ct);
        return payload!;
    }

    public async Task UpdateMeSettingsFullAsync(UserSettingContracts req, CancellationToken ct = default)
    {
        var res = await _http.PutAsJsonAsync("api/Me/settings/full", req, ct);
        if (!res.IsSuccessStatusCode)
            throw await CreateApiExceptionAsync(res);
    }


    // -------------------------
    // Decks
    // -------------------------

    public async Task<IReadOnlyList<DeckListItem>> GetDecksAsync(CancellationToken ct = default)
    {
        using var response = await _http.GetAsync("/api/Decks", ct);

        if (!response.IsSuccessStatusCode)
            throw await CreateApiExceptionAsync(response);

        var payload = await response.Content.ReadFromJsonAsync<List<DeckListItem>>(JsonOptions, ct);
        return payload ?? new List<DeckListItem>();
    }

    // -------------------------
    // Study
    // -------------------------

    public async Task<NextQuestionResponse> GetNextQuestionAsync(Guid deckId, CancellationToken ct = default)
    {
        // Note: server-side we prefer validating query via DTO:
        // GET /api/study/next?deckId=...
        using var response = await _http.GetAsync($"/api/Study/next?deckId={deckId}", ct);

        if (!response.IsSuccessStatusCode)
            throw await CreateApiExceptionAsync(response);

        return await ReadSuccessJsonAsync<NextQuestionResponse>(response, "GET /api/study/next", ct);
    }

    public async Task<SubmitAnswerResponse> SubmitAnswerAsync(SubmitAnswerRequest req, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("/api/Study/answer", req, JsonOptions, ct);

        if (!response.IsSuccessStatusCode)
            throw await CreateApiExceptionAsync(response);

        return await ReadSuccessJsonAsync<SubmitAnswerResponse>(response, "POST /api/study/answer", ct);
    }

    public async Task<TodayProgressResponse> GetTodayProgressAsync(CancellationToken ct = default)
    {
        using var response = await _http.GetAsync("/api/Study/today", ct);

        if (!response.IsSuccessStatusCode)
            throw await CreateApiExceptionAsync(response);

        return await ReadSuccessJsonAsync<TodayProgressResponse>(response, "GET /api/study/today", ct);
    }
}