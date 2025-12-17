using System.Net.Http.Json;
using Shared.Dtos;

namespace Ui.Blazor.Services;

public sealed class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http) => _http = http;

    public Task<PingResponseDto?> PingAsync() =>
        _http.GetFromJsonAsync<PingResponseDto>("api/ping");
}
