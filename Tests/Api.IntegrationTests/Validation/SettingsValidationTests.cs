using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Xunit;

using Api.IntegrationTests.Infrastructure;

using Shared.Contracts.Me;

using Domain.Enums;

namespace Api.IntegrationTests.Validation;

public class SettingsValidationTests
    : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SettingsValidationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Update_settings_invalid_daily_goal_returns_field_error()
    {
        await AuthTestHelper.AuthenticateAsync(_client);

        var payload = new UserSettingContracts
        {
            Language = LanguageCode.En,
            FontScale = FontScale.Medium,
            AudioSpeed = AudioSpeed.Normal,
            DailyGoalMinutes = 0, // invalid (min = 1)
            Focus = StudyFocus.Civics,
            SilentMode = false
        };

        var response = await _client.PutAsJsonAsync(
            "/api/me/settings/full", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.ReadValidationProblemAsync();

        problem.Errors.Should()
            .ContainKey(nameof(UserSettingContracts.DailyGoalMinutes));
    }
}
