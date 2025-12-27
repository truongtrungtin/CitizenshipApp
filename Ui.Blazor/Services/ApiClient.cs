using System.Net.Http.Json;

using Shared.Dtos.Auth;
using Shared.Dtos.Me;

namespace Ui.Blazor.Services;

/// <summary>
/// Typed client for API endpoints used by the Blazor UI.
/// Authorization is added by <see cref="AuthHeaderHandler"/>.
/// </summary>
public sealed class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/auth/register", request, ct);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<AuthResponseDto>(cancellationToken: ct))
               ?? throw new InvalidOperationException("Empty response from /auth/register");
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/auth/login", request, ct);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<AuthResponseDto>(cancellationToken: ct))
               ?? throw new InvalidOperationException("Empty response from /auth/login");
    }

    public async Task<UserSettingsDto> GetMeSettingsAsync(CancellationToken ct = default)
    {
        return (await _http.GetFromJsonAsync<UserSettingsDto>("api/me/settings", ct))
               ?? throw new InvalidOperationException("Empty response from GET /api/me/settings");
    }

    public async Task UpdateMeSettingsAsync(UserSettingsDto request, CancellationToken ct = default)
    {
        // API returns 204 NoContent
        using var response = await _http.PutAsJsonAsync("api/me/settings", request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task CompleteOnboardingAsync(CancellationToken ct = default)
    {
        // Backend: PUT /api/me/onboarding/complete
        using var response = await _http.PutAsync("api/me/onboarding/complete", content: null, ct);
        response.EnsureSuccessStatusCode();
    }
}
