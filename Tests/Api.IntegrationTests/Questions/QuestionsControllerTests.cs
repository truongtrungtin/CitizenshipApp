using System.Net;
using System.Net.Http.Json;

using Api.IntegrationTests.Infrastructure;

using Domain.Entities.Deck;
using DeckQuestion = Domain.Entities.Deck.Question;

using Infrastructure.Persistence;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using QuestionContract = Shared.Contracts.Deck.Question;

namespace Api.IntegrationTests.Questions;

public sealed class QuestionsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public QuestionsControllerTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetById_ReturnsQuestion()
    {
        using var client = _factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client, _factory.Services);

        Guid questionId = await SeedQuestionAsync();

        var res = await client.GetAsync($"/api/questions/{questionId}");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var payload = await res.Content.ReadFromJsonAsync<QuestionContract>();
        Assert.NotNull(payload);
        Assert.Equal(questionId, payload.Id);
    }

    [Fact]
    public async Task GetById_ReturnsProblemDetails_WhenNotFound()
    {
        using var client = _factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client, _factory.Services);

        var res = await client.GetAsync($"/api/questions/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);

        var problem = await res.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
    }

    private async Task<Guid> SeedQuestionAsync()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Guid deckId = Guid.NewGuid();
        Guid questionId = Guid.NewGuid();

        var deck = new Deck
        {
            DeckId = deckId,
            Code = $"TEST-{Guid.NewGuid():N}".Substring(0, 12),
            Name = $"Test Deck {Guid.NewGuid():N}".Substring(0, 20),
            IsActive = true
        };

        var question = new DeckQuestion
        {
            QuestionId = questionId,
            DeckId = deckId,
            Type = "MCQ",
            PromptEn = "What is 1+1?",
            PromptVi = "1+1 bằng mấy?",
            CorrectOptionKey = "A",
            Options = new List<QuestionOption>
            {
                new() { Key = "A", TextEn = "2", TextVi = "2", SortOrder = 1 },
                new() { Key = "B", TextEn = "3", TextVi = "3", SortOrder = 2 }
            }
        };

        deck.Questions.Add(question);
        db.Decks.Add(deck);
        await db.SaveChangesAsync();

        return questionId;
    }
}
