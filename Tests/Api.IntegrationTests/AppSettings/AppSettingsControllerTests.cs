using System.Net;
using System.Net.Http.Json;

using Api.IntegrationTests.Infrastructure;

using Microsoft.AspNetCore.Mvc;

using Shared.Contracts.AppSettings;

namespace Api.IntegrationTests.AppSettings;

public sealed class AppSettingsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AppSettingsControllerTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Crud_Works_ForAdmin()
    {
        using var client = _factory.CreateClient();
        await AuthTestHelper.AuthenticateAsAdminAsync(client, _factory.Services);

        // Create
        var create = new
        {
            key = $"feature_{Guid.NewGuid():N}",
            value = "true"
        };

        var createRes = await client.PostAsJsonAsync("/api/appsettings", create);
        await EnsureStatusAsync(createRes, HttpStatusCode.OK);

        var created = await createRes.Content.ReadFromJsonAsync<AppSettingDto>();
        Assert.NotNull(created);
        Assert.Equal(create.key, created.Key);

        // Get
        var getRes = await client.GetAsync($"/api/appsettings/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);

        // Update
        var update = new
        {
            key = created.Key,
            value = "false"
        };

        var updateRes = await client.PutAsJsonAsync($"/api/appsettings/{created.Id}", update);
        Assert.Equal(HttpStatusCode.OK, updateRes.StatusCode);

        // Delete
        var deleteRes = await client.DeleteAsync($"/api/appsettings/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenKeyExists()
    {
        using var client = _factory.CreateClient();
        await AuthTestHelper.AuthenticateAsAdminAsync(client, _factory.Services);

        var create = new
        {
            key = $"dupe_{Guid.NewGuid():N}",
            value = "true"
        };

        var first = await client.PostAsJsonAsync("/api/appsettings", create);
        await EnsureStatusAsync(first, HttpStatusCode.OK);

        var second = await client.PostAsJsonAsync("/api/appsettings", create);
        await EnsureStatusAsync(second, HttpStatusCode.Conflict);

        var problem = await second.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
    }

    private static async Task EnsureStatusAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        if (response.StatusCode == expected)
        {
            return;
        }

        string body = await response.Content.ReadAsStringAsync();
        throw new Xunit.Sdk.XunitException(
            $"Expected {(int)expected} {expected} but got {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
    }
}
