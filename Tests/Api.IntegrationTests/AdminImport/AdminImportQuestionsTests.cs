using System.Net;
using System.Net.Http.Json;

using Api.IntegrationTests.Infrastructure;

using Shared.Contracts.AdminImport;

namespace Api.IntegrationTests.AdminImport;

public sealed class AdminImportQuestionsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AdminImportQuestionsTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task ImportQuestions_RequiresAdminRole()
    {
        using var client = _factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client, _factory.Services);

        AdminImportQuestionsRequest request = BuildSampleRequest($"2025.12-{Guid.NewGuid():N}");

        var res = await client.PostAsJsonAsync("/api/admin/import/questions", request);
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task ImportQuestions_UpsertsByNaturalKey()
    {
        using var client = _factory.CreateClient();
        await AuthTestHelper.AuthenticateAsAdminAsync(client, _factory.Services);

        string testVersion = $"2025.12-{Guid.NewGuid():N}";
        AdminImportQuestionsRequest request = BuildSampleRequest(testVersion);

        var first = await client.PostAsJsonAsync("/api/admin/import/questions", request);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        AdminImportQuestionsResult? firstResult =
            await first.Content.ReadFromJsonAsync<AdminImportQuestionsResult>();
        Assert.NotNull(firstResult);
        Assert.Equal(2, firstResult.CreatedCount);
        Assert.Equal(0, firstResult.UpdatedCount);
        Assert.Equal(0, firstResult.ErrorCount);

        var second = await client.PostAsJsonAsync("/api/admin/import/questions", request);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        AdminImportQuestionsResult? secondResult =
            await second.Content.ReadFromJsonAsync<AdminImportQuestionsResult>();
        Assert.NotNull(secondResult);
        Assert.Equal(0, secondResult.CreatedCount);
        Assert.Equal(2, secondResult.UpdatedCount);
        Assert.Equal(0, secondResult.ErrorCount);
    }

    [Fact]
    public async Task ImportQuestions_InvalidItem_IsSkippedButBatchContinues()
    {
        using var client = _factory.CreateClient();
        await AuthTestHelper.AuthenticateAsAdminAsync(client, _factory.Services);

        string testVersion = $"2025.12-{Guid.NewGuid():N}";

        var items = new List<AdminImportQuestionItem>
        {
            new(
            testVersion,
                1,
                "MCQ",
                "What is 1+1?",
                null,
                null,
                null,
                null,
                "A",
                new List<AdminImportQuestionOptionItem>
                {
                    new("A", "2", null, 1),
                    new("B", "3", null, 2)
                }
            ),
            new(
                testVersion,
                2,
                "MCQ",
                "",
                null,
                null,
                null,
                null,
                "A",
                new List<AdminImportQuestionOptionItem>
                {
                    new("A", "2", null, 1),
                    new("B", "3", null, 2)
                }
            )
        };

        AdminImportQuestionsRequest request = new(items);

        var res = await client.PostAsJsonAsync("/api/admin/import/questions", request);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        AdminImportQuestionsResult? result =
            await res.Content.ReadFromJsonAsync<AdminImportQuestionsResult>();
        Assert.NotNull(result);
        Assert.Equal(1, result.CreatedCount);
        Assert.Equal(1, result.SkippedCount);
        Assert.True(result.ErrorCount > 0);
    }

    private static AdminImportQuestionsRequest BuildSampleRequest(string testVersion)
    {
        var items = new List<AdminImportQuestionItem>
        {
            new(
                testVersion,
                1,
                "MCQ",
                "What is 1+1?",
                null,
                null,
                null,
                null,
                "A",
                new List<AdminImportQuestionOptionItem>
                {
                    new("A", "2", null, 1),
                    new("B", "3", null, 2)
                }
            ),
            new(
                testVersion,
                2,
                "MCQ",
                "What is 2+2?",
                null,
                null,
                null,
                null,
                "A",
                new List<AdminImportQuestionOptionItem>
                {
                    new("A", "4", null, 1),
                    new("B", "5", null, 2)
                }
            )
        };

        return new AdminImportQuestionsRequest(items);
    }
}
