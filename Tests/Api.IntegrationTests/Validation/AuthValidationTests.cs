using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Xunit;

using Api.IntegrationTests.Infrastructure;

using Shared.Contracts.Auth;

namespace Api.IntegrationTests.Validation;

public class AuthValidationTests
    : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthValidationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_invalid_payload_returns_field_errors()
    {
        var payload = new RegisterRequest
        {
            Email = "not-an-email",
            Password = "" // invalid
        };

        var response = await _client.PostAsJsonAsync(
            "/api/auth/register", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.ReadValidationProblemAsync();

        problem.Errors.Should().ContainKey(nameof(RegisterRequest.Email));
        problem.Errors.Should().ContainKey(nameof(RegisterRequest.Password));
    }

    [Fact]
    public async Task Login_missing_password_returns_field_error()
    {
        var payload = new LoginRequest
        {
            Email = "test@example.com",
            Password = ""
        };

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.ReadValidationProblemAsync();

        problem.Errors.Should().ContainKey(nameof(LoginRequest.Password));
    }
}
