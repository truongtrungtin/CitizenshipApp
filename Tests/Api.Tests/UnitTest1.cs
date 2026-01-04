using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;

using Shared.Contracts.Deck;

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

public sealed class ApiDeckQuestionQueryTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public ApiDeckQuestionQueryTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Decks_can_be_listed_from_db()
    {
        using HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "1");
        client.DefaultRequestHeaders.Add("X-Test-UserId", Guid.NewGuid().ToString());

        HttpResponseMessage resp = await client.GetAsync("/api/decks");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        List<DeckSummary>? payload = await resp.Content.ReadFromJsonAsync<List<DeckSummary>>();
        payload.Should().NotBeNull();
        payload!.Should().Contain(d => d.Id == _factory.SeedDeckId);
    }

    [Fact]
    public async Task Question_by_id_is_served_from_db()
    {
        using HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "1");
        client.DefaultRequestHeaders.Add("X-Test-UserId", Guid.NewGuid().ToString());

        HttpResponseMessage resp = await client.GetAsync($"/api/questions/{_factory.SeedQuestionId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        Question? payload = await resp.Content.ReadFromJsonAsync<Question>();
        payload.Should().NotBeNull();
        payload!.Id.Should().Be(_factory.SeedQuestionId);
        payload.DeckId.Should().Be(_factory.SeedDeckId);
        payload.Options.Should().NotBeNull();
        payload.Options.Should().Contain(o => o.Key == "B" && o.TextEn == "4");
    }

    [Fact]
    public async Task Unknown_question_returns_404()
    {
        using HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Auth", "1");
        client.DefaultRequestHeaders.Add("X-Test-UserId", Guid.NewGuid().ToString());

        HttpResponseMessage resp = await client.GetAsync($"/api/questions/{Guid.NewGuid()}");

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
