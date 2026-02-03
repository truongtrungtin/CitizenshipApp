using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Api.IntegrationTests.Infrastructure;

using Domain.Entities.Deck;

using Infrastructure.Persistence;

using Microsoft.Extensions.DependencyInjection;

using Shared.Contracts.Auth;
using Shared.Contracts.Study;

namespace Api.IntegrationTests.StudyFlow;

/// <summary>
/// BL-023: End-to-end integration tests for Study flow.
/// Covers:
/// - Auth requirement on study endpoints
/// - /api/study/next returns a question when deck has questions
/// - /api/study/answer records answer and returns correctness
/// - /api/study/today returns progress
/// </summary>
public sealed class StudyFlowTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public StudyFlowTests(TestWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task StudyNext_RequiresAuth()
    {
        using var client = _factory.CreateClient();

        var res = await client.GetAsync("/api/study/next?deckId=11111111-1111-1111-1111-111111111111");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task StudyNext_ReturnsQuestion_WhenDeckHasQuestions()
    {
        using var client = _factory.CreateClient();

        AuthResponse auth = await RegisterAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        (Guid deckId, Guid q1, Guid q2) = await SeedDeckWithTwoQuestionsAsync(correctKey: "A");

        NextQuestionResponse? next = await client.GetFromJsonAsync<NextQuestionResponse>($"/api/study/next?deckId={deckId}");

        Assert.NotNull(next);
        Assert.NotNull(next!.Question);

        // Shared.Contracts.Deck.Question is a record with Id/DeckId/Type/Prompt/Options
        Assert.Equal(deckId, next.Question.DeckId);
        Assert.True(next.Question.Id == q1 || next.Question.Id == q2);

        Assert.NotNull(next.Question.Options);
        Assert.True(next.Question.Options.Count >= 2);
        Assert.Contains(next.Question.Options, o => o.Key == "A");
        Assert.Contains(next.Question.Options, o => o.Key == "B");
    }

    [Fact]
    public async Task StudyAnswer_TodayProgressIncrements_AfterSubmittingCorrectAnswer()
    {
        using var client = _factory.CreateClient();

        AuthResponse auth = await RegisterAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        (Guid deckId, _, _) = await SeedDeckWithTwoQuestionsAsync(correctKey: "A");

        // 1) Get a real question from the API (so we don't depend on seed assumptions)
        NextQuestionResponse? next = await client.GetFromJsonAsync<NextQuestionResponse>($"/api/study/next?deckId={deckId}");
        Assert.NotNull(next);
        Assert.NotNull(next!.Question);
        Assert.True(next.Question.Options.Count >= 2);

        Guid questionId = next.Question.Id;

        // Pick a key that is very likely wrong: choose the last option first,
        // then we will use CorrectKey from the response to submit the correct one.
        string wrongKey = next.Question.Options.Last().Key;

        // 2) Submit wrong answer -> must return CorrectKey
        var wrongReq = new SubmitAnswerRequest
        {
            DeckId = deckId,
            QuestionId = questionId,
            SelectedKey = wrongKey
        };

        HttpResponseMessage wrongRes = await client.PostAsJsonAsync("/api/study/answer", wrongReq);
        Assert.Equal(HttpStatusCode.OK, wrongRes.StatusCode);

        SubmitAnswerResponse? wrongPayload = await wrongRes.Content.ReadFromJsonAsync<SubmitAnswerResponse>();
        Assert.NotNull(wrongPayload);

        if (wrongPayload!.IsCorrect)
        {
            string altWrong = next.Question.Options.First().Key;

            var wrongReq2 = new SubmitAnswerRequest
            {
                DeckId = deckId,
                QuestionId = questionId,
                SelectedKey = altWrong
            };

            HttpResponseMessage wrongRes2 = await client.PostAsJsonAsync("/api/study/answer", wrongReq2);
            Assert.Equal(HttpStatusCode.OK, wrongRes2.StatusCode);

            wrongPayload = await wrongRes2.Content.ReadFromJsonAsync<SubmitAnswerResponse>();
            Assert.NotNull(wrongPayload);
        }

        Assert.False(wrongPayload!.IsCorrect);
        Assert.False(string.IsNullOrWhiteSpace(wrongPayload.CorrectKey));

        // 3) Submit correct answer using CorrectKey returned by API
        var correctReq = new SubmitAnswerRequest
        {
            DeckId = deckId,
            QuestionId = questionId,
            SelectedKey = wrongPayload.CorrectKey!
        };

        HttpResponseMessage correctRes = await client.PostAsJsonAsync("/api/study/answer", correctReq);
        Assert.Equal(HttpStatusCode.OK, correctRes.StatusCode);

        SubmitAnswerResponse? correctPayload = await correctRes.Content.ReadFromJsonAsync<SubmitAnswerResponse>();
        Assert.NotNull(correctPayload);
        Assert.True(correctPayload!.IsCorrect);

        // 4) Today progress should reflect at least one answered (and at least one correct)
        TodayProgressResponse? today = await client.GetFromJsonAsync<TodayProgressResponse>("/api/study/today");
        Assert.NotNull(today);

        Assert.True(today!.DailyGoalMinutes > 0);
        Assert.True(today.TotalAnswered >= 2);       // we submitted twice (wrong + correct)
        Assert.True(today.CorrectAnswered >= 1);
        Assert.True(today.CorrectAnswered <= today.TotalAnswered);
    }
    [Fact]
    public async Task StudyAnswer_ReturnsCorrectKey_WhenWrong()
    {
        using var client = _factory.CreateClient();

        AuthResponse auth = await RegisterAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        (Guid deckId, Guid q1, _) = await SeedDeckWithTwoQuestionsAsync(correctKey: "A");

        // Submit wrong answer "B"
        var submit = new SubmitAnswerRequest
        {
            DeckId = deckId,
            QuestionId = q1,
            SelectedKey = "B"
        };

        var res = await client.PostAsJsonAsync("/api/study/answer", submit);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        SubmitAnswerResponse? payload = await res.Content.ReadFromJsonAsync<SubmitAnswerResponse>();
        Assert.NotNull(payload);

        Assert.False(payload!.IsCorrect);

        // Service contract: CorrectKey should be returned when wrong (your comment says so)
        Assert.Equal("A", payload.CorrectKey);
    }

    /// <summary>
    /// Register a new user via the real API and return AuthResponse.
    /// Uses unique email to avoid collisions.
    /// </summary>
    private static async Task<AuthResponse> RegisterAsync(HttpClient client)
    {
        string email = $"{Guid.NewGuid():N}@test.local";
        const string password = "Passw0rd!123";

        var req = new RegisterRequest
        {
            Email = email,
            Password = password
        };

        HttpResponseMessage res = await client.PostAsJsonAsync("/api/auth/register", req);

        if (!res.IsSuccessStatusCode)
        {
            string txt = await res.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Register failed: {(int)res.StatusCode} {res.ReasonPhrase}\n{txt}");
        }

        AuthResponse? auth = await res.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth is null || string.IsNullOrWhiteSpace(auth.AccessToken))
            throw new InvalidOperationException("Register did not return a valid access token.");

        return auth;
    }

    /// <summary>
    /// Seed 1 deck + 2 questions + options.
    /// Uses unique deck code/name each time to avoid unique index collisions (Decks.Code).
    /// </summary>
    private async Task<(Guid deckId, Guid q1, Guid q2)> SeedDeckWithTwoQuestionsAsync(string correctKey)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Guid deckId = Guid.NewGuid();
        Guid q1 = Guid.NewGuid();
        Guid q2 = Guid.NewGuid();

        string code = $"TEST-{Guid.NewGuid():N}".Substring(0, 12);
        string name = $"Test Deck {Guid.NewGuid():N}".Substring(0, 20);
        string testVersion = $"2025.12-{Guid.NewGuid():N}";

        var deck = new Deck
        {
            DeckId = deckId,
            Code = code,
            Name = name
        };

        var question1 = new Question
        {
            QuestionId = q1,
            DeckId = deckId,
            TestVersion = testVersion,
            QuestionNo = 1,
            Type = "MCQ", // your mapper supports "MCQ" => SingleChoice
            PromptEn = "What is 1+1?",
            PromptVi = "1+1 bằng mấy?",
            PromptViPhonetic = null,
            ExplainEn = "1+1 equals 2.",
            ExplainVi = "1+1 bằng 2.",
            CorrectOptionKey = correctKey
        };

        var question2 = new Question
        {
            QuestionId = q2,
            DeckId = deckId,
            TestVersion = testVersion,
            QuestionNo = 2,
            Type = "MCQ",
            PromptEn = "What is 2+2?",
            PromptVi = "2+2 bằng mấy?",
            PromptViPhonetic = null,
            ExplainEn = "2+2 equals 4.",
            ExplainVi = "2+2 bằng 4.",
            CorrectOptionKey = correctKey
        };

        var q1OptA = new QuestionOption
        {
            QuestionId = q1,
            Key = "A",
            TextEn = "2",
            TextVi = "2",
            SortOrder = 1
        };
        var q1OptB = new QuestionOption
        {
            QuestionId = q1,
            Key = "B",
            TextEn = "3",
            TextVi = "3",
            SortOrder = 2
        };

        var q2OptA = new QuestionOption
        {
            QuestionId = q2,
            Key = "A",
            TextEn = "4",
            TextVi = "4",
            SortOrder = 1
        };
        var q2OptB = new QuestionOption
        {
            QuestionId = q2,
            Key = "B",
            TextEn = "5",
            TextVi = "5",
            SortOrder = 2
        };

        db.Decks.Add(deck);
        db.Questions.AddRange(question1, question2);
        db.QuestionOptions.AddRange(q1OptA, q1OptB, q2OptA, q2OptB);

        await db.SaveChangesAsync();

        return (deckId, q1, q2);
    }
}
