using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Api.IntegrationTests.Infrastructure;

using Shared.Contracts.Auth;
using Shared.Contracts.Me;

namespace Api.IntegrationTests.AuthFlow;

public sealed class AuthFlowTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory = factory;

    private static string NewEmail() => $"u{Guid.NewGuid():N}@test.local";

    private static async Task<AuthResponse> RegisterAsync(HttpClient client, string email, string password)
    {
        var req = new RegisterRequest
        {
            Email = email,
            Password = password
        };

        var resp = await client.PostAsJsonAsync("/api/auth/register", req);

        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException(
                $"Register failed: {(int)resp.StatusCode} {resp.StatusCode}\n{body}");
        }

        var dto = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(dto);
        return dto!;
    }

    private static async Task<AuthResponse> LoginAsync(HttpClient client, string email, string password)
    {
        var req = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var resp = await client.PostAsJsonAsync("/api/auth/login", req);

        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException(
                $"Login failed: {(int)resp.StatusCode} {resp.StatusCode}\n{body}");
        }

        var dto = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(dto);
        return dto!;
    }

    private static void SetBearer(HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    [Fact]
    public async Task Register_ReturnsToken_UserId_IsOnboardedFalse()
    {
        using var client = _factory.CreateClient();
        string email = NewEmail();
        const string password = "P@ssw0rd!123";

        AuthResponse auth = await RegisterAsync(client, email, password);

        Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));
        Assert.NotEqual(Guid.Empty, auth.UserId);
        Assert.False(auth.IsOnboarded);
    }

    [Fact]
    public async Task Login_ReturnsToken_UserId_IsOnboardedFalse()
    {
        using var client = _factory.CreateClient();
        string email = NewEmail();
        const string password = "P@ssw0rd!123";

        AuthResponse reg = await RegisterAsync(client, email, password);
        AuthResponse login = await LoginAsync(client, email, password);

        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
        Assert.Equal(reg.UserId, login.UserId);
        Assert.False(login.IsOnboarded);
    }

    [Fact]
    public async Task MeProfile_Returns401_WithoutToken()
    {
        using var client = _factory.CreateClient();

        var resp = await client.GetAsync("/api/me/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task MeProfile_Returns200_WithToken()
    {
        using var client = _factory.CreateClient();
        string email = NewEmail();
        const string password = "P@ssw0rd!123";

        AuthResponse reg = await RegisterAsync(client, email, password);
        SetBearer(client, reg.AccessToken);

        var resp = await client.GetAsync("/api/me/profile");

        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException(
                $"MeProfile failed: {(int)resp.StatusCode} {resp.StatusCode}\n{body}");
        }

        var profile = await resp.Content.ReadFromJsonAsync<MeProfileResponse>();

        Assert.NotNull(profile);
        Assert.Equal(reg.UserId, profile!.UserId);
        Assert.Equal(email, profile.Email);
        Assert.False(profile.IsOnboarded);
    }

    [Fact]
    public async Task CompleteOnboarding_Then_LoginShowsIsOnboardedTrue()
    {
        using var client = _factory.CreateClient();
        string email = NewEmail();
        const string password = "P@ssw0rd!123";

        AuthResponse reg = await RegisterAsync(client, email, password);
        SetBearer(client, reg.AccessToken);

        var completeResp = await client.PutAsync("/api/me/onboarding/complete", content: null);
        Assert.Equal(HttpStatusCode.NoContent, completeResp.StatusCode);

        AuthResponse login = await LoginAsync(client, email, password);
        Assert.True(login.IsOnboarded);
    }
}
