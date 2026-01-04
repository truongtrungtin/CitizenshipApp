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
/// </summary>
public sealed class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;

    public ApiClient(HttpClient http) => _http = http;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("/api/auth/register", request, JsonOptions, ct);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions, ct))
               ?? throw new InvalidOperationException("Empty response from /api/auth/register");
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("/api/auth/login", request, JsonOptions, ct);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions, ct))
               ?? throw new InvalidOperationException("Empty response from /api/auth/login");
    }

    public async Task<MeSettingsResponse> GetMeSettingsAsync(CancellationToken ct = default)
    {
        var res = await _http.GetFromJsonAsync<MeSettingsResponse>("/api/me/settings", JsonOptions, ct);
        return res ?? throw new InvalidOperationException("GET /api/me/settings returned empty body.");
    }

    public async Task<MeSettingsResponse> UpdateMeSettingsAsync(UpdateMeSettingsRequest request, CancellationToken ct = default)
    {
        using var resp = await _http.PutAsJsonAsync("/api/me/settings", request, JsonOptions, ct);
        resp.EnsureSuccessStatusCode();

        return (await resp.Content.ReadFromJsonAsync<MeSettingsResponse>(JsonOptions, ct))
               ?? throw new InvalidOperationException("PUT /api/me/settings returned empty body.");
    }

    public async Task CompleteOnboardingAsync(CancellationToken ct = default)
    {
        using var response = await _http.PutAsync("/api/me/onboarding/complete", content: null, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<DeckListItem>> GetDecksAsync(CancellationToken ct = default)
    {
        var items = await _http.GetFromJsonAsync<List<DeckListItem>>("/api/decks", JsonOptions, ct);
        return items ?? new List<DeckListItem>();
    }

    public async Task<NextQuestionResponse> GetNextQuestionAsync(Guid deckId, CancellationToken ct = default)
    {
        var res = await _http.GetFromJsonAsync<NextQuestionResponse>($"/api/study/next?deckId={deckId}", JsonOptions, ct);
        return res ?? throw new InvalidOperationException("GET /api/study/next returned empty body.");
    }

    public async Task<SubmitAnswerResponse> SubmitAnswerAsync(SubmitAnswerRequest req, CancellationToken ct = default)
    {
        using var resp = await _http.PostAsJsonAsync("/api/study/answer", req, JsonOptions, ct);
        resp.EnsureSuccessStatusCode();

        return (await resp.Content.ReadFromJsonAsync<SubmitAnswerResponse>(JsonOptions, ct))
               ?? throw new InvalidOperationException("POST /api/study/answer returned empty body.");
    }

    public async Task<TodayProgressResponse> GetTodayProgressAsync(CancellationToken ct = default)
    {
        var res = await _http.GetFromJsonAsync<TodayProgressResponse>("/api/study/today", JsonOptions, ct);
        return res ?? throw new InvalidOperationException("GET /api/study/today returned empty body.");
    }
}
