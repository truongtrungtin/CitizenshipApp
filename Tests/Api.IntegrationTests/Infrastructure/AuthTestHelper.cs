using System.Net.Http.Headers;
using System.Net.Http.Json;

using Shared.Contracts.Auth;

namespace Api.IntegrationTests.Infrastructure;

public static class AuthTestHelper
{
    public static async Task AuthenticateAsync(HttpClient client)
    {
        await client.PostAsJsonAsync("/api/Auth/register",
            new RegisterRequest
            {
                Email = "settings@test.com",
                Password = "Password123!"
            });

        var login = await client.PostAsJsonAsync("/api/Auth/login",
            new LoginRequest
            {
                Email = "settings@test.com",
                Password = "Password123!"
            });

        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
    }
}
