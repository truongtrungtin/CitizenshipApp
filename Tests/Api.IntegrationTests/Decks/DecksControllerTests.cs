using System.Net;
using System.Net.Http.Json;

using Api.IntegrationTests.Infrastructure;

using Domain.Entities.Deck;

using Infrastructure.Persistence;

using Microsoft.Extensions.DependencyInjection;

using Shared.Contracts.Deck;
using QuestionContract = Shared.Contracts.Deck.Question;

namespace Api.IntegrationTests.Decks;

public sealed class DecksControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public DecksControllerTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetAll_RequiresAuth()
    {
        using var client = _factory.CreateClient();

        var res = await client.GetAsync("/api/decks");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsDecks()
    {
        using var client = _factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client, _factory.Services);

        await SeedDeckWithQuestionsAsync();

        var res = await client.GetAsync("/api/decks");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var payload = await res.Content.ReadFromJsonAsync<List<DeckSummary>>();
        Assert.NotNull(payload);
        Assert.NotEmpty(payload);
    }

    [Fact]
    public async Task GetDeck_ReturnsDeckById()
    {
        using var client = _factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client, _factory.Services);

        Guid deckId = await SeedDeckWithQuestionsAsync();

        var res = await client.GetAsync($"/api/decks/{deckId}");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var payload = await res.Content.ReadFromJsonAsync<DeckListItem>();
        Assert.NotNull(payload);
        Assert.Equal(deckId, payload.Id);
    }

    [Fact]
    public async Task GetDeckQuestions_ReturnsQuestions()
    {
        using var client = _factory.CreateClient();
        await AuthTestHelper.AuthenticateAsync(client, _factory.Services);

        Guid deckId = await SeedDeckWithQuestionsAsync();

        var res = await client.GetAsync($"/api/decks/{deckId}/questions?page=1&pageSize=50");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var payload = await res.Content.ReadFromJsonAsync<List<QuestionContract>>();
        Assert.NotNull(payload);
        Assert.NotEmpty(payload);
        Assert.All(payload, q => Assert.Equal(deckId, q.DeckId));
    }

    private async Task<Guid> SeedDeckWithQuestionsAsync()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Guid deckId = Guid.NewGuid();
        string code = $"TEST-{Guid.NewGuid():N}".Substring(0, 12);
        string name = $"Test Deck {Guid.NewGuid():N}".Substring(0, 20);

        var deck = new Deck
        {
            DeckId = deckId,
            Code = code,
            Name = name,
            IsActive = true
        };

        var q1 = new Domain.Entities.Deck.Question
        {
            QuestionId = Guid.NewGuid(),
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

        var q2 = new Domain.Entities.Deck.Question
        {
            QuestionId = Guid.NewGuid(),
            DeckId = deckId,
            Type = "MCQ",
            PromptEn = "What is 2+2?",
            PromptVi = "2+2 bằng mấy?",
            CorrectOptionKey = "A",
            Options = new List<QuestionOption>
            {
                new() { Key = "A", TextEn = "4", TextVi = "4", SortOrder = 1 },
                new() { Key = "B", TextEn = "5", TextVi = "5", SortOrder = 2 }
            }
        };

        deck.Questions.Add(q1);
        deck.Questions.Add(q2);

        db.Decks.Add(deck);
        await db.SaveChangesAsync();

        return deckId;
    }
}
