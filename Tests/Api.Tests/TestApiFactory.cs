using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Domain.Entities.Deck;

using Infrastructure.Persistence;

namespace Api.Tests;

public sealed class TestApiFactory : WebApplicationFactory<Program>
{
    private readonly IServiceProvider _inMemoryEfServiceProvider =
        new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

    private readonly string _databaseName = $"CitizenshipApp_Testing_{Guid.NewGuid():N}";

    public Guid SeedDeckId { get; } = Guid.Parse("1fa43ce0-9c77-4a92-b07a-8d7bd19f29e0");
    public Guid SeedQuestionId { get; } = Guid.Parse("9b1a089f-0af1-4c23-9d3c-0a845d22ab28");

    public TestApiFactory()
    {
        // IMPORTANT: Api/appsettings.Development.json contains empty placeholders.
        // Environment variables are loaded after JSON and will override those values.
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");

        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            "Server=localhost;Database=CitizenshipApp_Testing;User Id=sa;Password=Passw0rd!;TrustServerCertificate=True");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "CitizenshipApp");
        Environment.SetEnvironmentVariable("Jwt__Audience", "CitizenshipApp.Ui");
        Environment.SetEnvironmentVariable("Jwt__Key", new string('x', 64));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace SQL Server DbContext with InMemory so tests don't need a real DB.
            foreach (ServiceDescriptor d in services.Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)).ToList())
            {
                services.Remove(d);
            }

            foreach (ServiceDescriptor d in services.Where(d => d.ServiceType == typeof(AppDbContext)).ToList())
            {
                services.Remove(d);
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.UseInternalServiceProvider(_inMemoryEfServiceProvider);
            });

            // Replace auth with a controllable test scheme.
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });

        IHost host = base.CreateHost(builder);
        SeedDatabase(host.Services);
        return host;
    }

    private void SeedDatabase(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Database.EnsureCreated();

        // Idempotent seed.
        if (db.Decks.Any(d => d.DeckId == SeedDeckId))
        {
            return;
        }

        var deck = new Deck
        {
            DeckId = SeedDeckId,
            Code = "test-deck",
            Name = "Test Deck",
            IsActive = true
        };

        var question = new Question
        {
            QuestionId = SeedQuestionId,
            DeckId = SeedDeckId,
            Type = "MCQ",
            PromptEn = "What is 2 + 2?",
            PromptVi = "2 + 2 bằng mấy?",
            CorrectOptionKey = "B",
            Options = new List<QuestionOption>
            {
                new()
                {
                    QuestionId = SeedQuestionId,
                    Key = "A",
                    TextEn = "3",
                    TextVi = "3",
                    SortOrder = 1
                },
                new()
                {
                    QuestionId = SeedQuestionId,
                    Key = "B",
                    TextEn = "4",
                    TextVi = "4",
                    SortOrder = 2
                }
            }
        };

        db.Decks.Add(deck);
        db.Questions.Add(question);
        db.SaveChanges();
    }
}
