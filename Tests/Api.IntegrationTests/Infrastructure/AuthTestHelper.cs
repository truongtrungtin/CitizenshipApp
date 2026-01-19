using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

using Shared.Contracts.Auth;

namespace Api.IntegrationTests.Infrastructure;

public static class AuthTestHelper
{
    public static async Task AuthenticateAsync(HttpClient client)
    {
        // Generate unique username/email to avoid collisions across tests/runs.
        var unique = Guid.NewGuid().ToString("N")[..8];
        var username = $"test_{unique}";
        var email = $"{username}@test.local";
        var password = "Test123!@#abc"; // must satisfy Identity password policy

        // 1) Register
        var registerRes = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = email,
            Password = password
        });

        registerRes.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        // 2) Login
        var loginRes = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });

        loginRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var login = await loginRes.Content.ReadFromJsonAsync<AuthResponse>();
        login.Should().NotBeNull("login response must contain an access token");
        login!.AccessToken.Should().NotBeNullOrWhiteSpace("login response must contain an access token");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login!.AccessToken);
    }
}
