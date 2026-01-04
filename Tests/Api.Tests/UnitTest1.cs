using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

namespace Api.Tests;

public sealed class ApiSecurityTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public ApiSecurityTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AppSettings_requires_authentication()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage resp = await client.GetAsync("/api/appsettings");

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AppSettings_requires_admin_role()
    {
        using HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "1");
        client.DefaultRequestHeaders.Add("X-Test-UserId", Guid.NewGuid().ToString());

        HttpResponseMessage resp = await client.GetAsync("/api/appsettings");

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UnauthorizedAccessException_is_returned_as_problem_details_401()
    {
        using HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "1");

        // Authenticated, but no NameIdentifier claim -> StudyController throws UnauthorizedAccessException.
        HttpResponseMessage resp = await client.GetAsync("/api/study/today");

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        resp.Content.Headers.ContentType.Should().NotBeNull();
        resp.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var payload = await resp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        payload.Should().NotBeNull();
        payload!.Should().ContainKey("title");
        payload.Should().ContainKey("traceId");
    }
}
