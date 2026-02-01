using System.Net;
using System.Net.Http.Json;

using Api.IntegrationTests.Infrastructure;

using Shared.Contracts.Auth;
using Shared.Contracts.Me;

namespace Api.IntegrationTests.MeFlow;

public sealed class MeControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public MeControllerTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Profile_Returns401_WhenUnauthorized()
    {
        using var client = _factory.CreateClient();

        var res = await client.GetAsync("/api/me/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Profile_ReturnsProfile_WhenAuthorized()
    {
        using var client = _factory.CreateClient();

        AuthResponse auth = await RegisterAsync(client);

        var res = await client.GetAsync("/api/me/profile");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var payload = await res.Content.ReadFromJsonAsync<MeProfileResponse>();
        Assert.NotNull(payload);
        Assert.Equal(auth.UserId, payload.UserId);
        Assert.False(payload.IsOnboarded);
    }

    [Fact]
    public async Task SettingsFull_ReturnsDefaults_WhenMissingSettings()
    {
        using var client = _factory.CreateClient();

        _ = await RegisterAsync(client);

        var res = await client.GetAsync("/api/me/settings/full");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var payload = await res.Content.ReadFromJsonAsync<UserSettingContracts>();
        Assert.NotNull(payload);
        Assert.Equal(Domain.Enums.LanguageCode.Vi, payload.Language);
        Assert.Equal(Domain.Enums.FontScale.Large, payload.FontScale);
        Assert.Equal(Domain.Enums.AudioSpeed.Slow, payload.AudioSpeed);
    }

    [Fact]
    public async Task SettingsFull_CanUpdateAndReadBack()
    {
        using var client = _factory.CreateClient();

        _ = await RegisterAsync(client);

        var update = new UserSettingContracts
        {
            Language = Domain.Enums.LanguageCode.En,
            FontScale = Domain.Enums.FontScale.Medium,
            AudioSpeed = Domain.Enums.AudioSpeed.Normal,
            DailyGoalMinutes = 20,
            Focus = Domain.Enums.StudyFocus.Civics,
            SilentMode = true
        };

        var put = await client.PutAsJsonAsync("/api/me/settings/full", update);
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var res = await client.GetAsync("/api/me/settings/full");
        var payload = await res.Content.ReadFromJsonAsync<UserSettingContracts>();
        Assert.NotNull(payload);
        Assert.Equal(update.Language, payload.Language);
        Assert.Equal(update.FontScale, payload.FontScale);
        Assert.Equal(update.AudioSpeed, payload.AudioSpeed);
        Assert.Equal(update.DailyGoalMinutes, payload.DailyGoalMinutes);
        Assert.Equal(update.Focus, payload.Focus);
        Assert.Equal(update.SilentMode, payload.SilentMode);
    }

    private static async Task<AuthResponse> RegisterAsync(HttpClient client)
    {
        string email = $"u{Guid.NewGuid():N}@test.local";
        const string password = "Passw0rd!123";

        var req = new RegisterRequest
        {
            Email = email,
            Password = password
        };

        var res = await client.PostAsJsonAsync("/api/auth/register", req);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var auth = await res.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.AccessToken);

        return auth;
    }
}
