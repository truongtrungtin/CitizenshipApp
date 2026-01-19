using System.Net.Http.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;

namespace Api.IntegrationTests.Infrastructure;

public static class HttpResponseExtensions
{
    public static async Task<ValidationProblemDetails> ReadValidationProblemAsync(
        this HttpResponseMessage response)
    {
        response.Content.Headers.ContentType!.MediaType
            .Should().Be("application/problem+json");

        var problem = await response.Content
            .ReadFromJsonAsync<ValidationProblemDetails>();

        problem.Should().NotBeNull();
        return problem!;
    }
}
