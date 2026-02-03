using Application.E2E;

using Domain.Entities.Deck;
using Domain.Entities.Users;

using Infrastructure.Identity;
using Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Shared.Contracts.E2E;

namespace Infrastructure.E2E;

public sealed class E2ESeedService(
    AppDbContext db,
    UserManager<AppUser> userManager,
    IConfiguration configuration)
    : IE2ESeedService
{
    private const string DeckCode = "civics-e2e";
    private const string DeckName = "US Civics (E2E)";

    public async Task<E2ESeedResponse> SeedAsync(CancellationToken ct)
    {
        var (email, password) = GetCredentials();

        AppUser user = await EnsureUserAsync(email, password);
        await EnsureProfileAndSettingsAsync(user.Id, ct);
        await EnsureDeckAsync(ct);

        var deck = await db.Decks
            .AsNoTracking()
            .FirstAsync(d => d.Code == DeckCode, ct);

        return new E2ESeedResponse
        {
            Email = email,
            Password = password,
            DeckId = deck.DeckId,
            DeckCode = deck.Code,
            DeckName = deck.Name
        };
    }

    private (string Email, string Password) GetCredentials()
    {
        var email = configuration["Seed:E2EUserEmail"]?.Trim();
        var password = configuration["Seed:E2EUserPassword"]?.Trim();

        return (
            string.IsNullOrWhiteSpace(email) ? "e2e_user@example.com" : email,
            string.IsNullOrWhiteSpace(password) ? "ChangeMe123!" : password
        );
    }

    private async Task<AppUser> EnsureUserAsync(string email, string password)
    {
        AppUser? user = await userManager.FindByNameAsync(email);
        if (user is null)
        {
            user = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            IdentityResult created = await userManager.CreateAsync(user, password);
            if (!created.Succeeded)
            {
                string msg = string.Join(" ", created.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"E2E user create failed: {msg}");
            }
        }
        else
        {
            bool ok = await userManager.CheckPasswordAsync(user, password);
            if (!ok)
            {
                string token = await userManager.GeneratePasswordResetTokenAsync(user);
                IdentityResult reset = await userManager.ResetPasswordAsync(user, token, password);
                if (!reset.Succeeded)
                {
                    string msg = string.Join(" ", reset.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"E2E user password reset failed: {msg}");
                }
            }
        }

        return user;
    }

    private async Task EnsureProfileAndSettingsAsync(Guid userId, CancellationToken ct)
    {
        UserProfile? profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile is null)
        {
            var now = DateTime.UtcNow;
            profile = new UserProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                IsOnboarded = true,
                CreatedUtc = now,
                UpdatedUtc = now
            };
            db.UserProfiles.Add(profile);
        }
        else if (!profile.IsOnboarded)
        {
            profile.IsOnboarded = true;
            profile.UpdatedUtc = DateTime.UtcNow;
        }

        UserSettings? settings = await db.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId, ct);
        if (settings is null)
        {
            settings = new UserSettings
            {
                Id = profile.Id,
                UserId = userId,
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };
            db.UserSettings.Add(settings);
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task EnsureDeckAsync(CancellationToken ct)
    {
        if (await db.Decks.AnyAsync(d => d.Code == DeckCode, ct))
        {
            return;
        }

        var deck = new Deck
        {
            Code = DeckCode,
            Name = DeckName,
            IsActive = true
        };

        var q1 = new Question
        {
            Deck = deck,
            TestVersion = "2025.12",
            QuestionNo = 1,
            Type = "MCQ",
            PromptEn = "What is 1 + 1?",
            PromptVi = "1 + 1 bằng mấy?",
            ExplainEn = "1 + 1 equals 2.",
            ExplainVi = "1 + 1 bằng 2.",
            CorrectOptionKey = "A",
            Options =
            {
                new QuestionOption { Key = "A", TextEn = "2", TextVi = "2", SortOrder = 1 },
                new QuestionOption { Key = "B", TextEn = "3", TextVi = "3", SortOrder = 2 }
            }
        };

        var q2 = new Question
        {
            Deck = deck,
            TestVersion = "2025.12",
            QuestionNo = 2,
            Type = "MCQ",
            PromptEn = "What is 2 + 2?",
            PromptVi = "2 + 2 bằng mấy?",
            ExplainEn = "2 + 2 equals 4.",
            ExplainVi = "2 + 2 bằng 4.",
            CorrectOptionKey = "A",
            Options =
            {
                new QuestionOption { Key = "A", TextEn = "4", TextVi = "4", SortOrder = 1 },
                new QuestionOption { Key = "B", TextEn = "5", TextVi = "5", SortOrder = 2 }
            }
        };

        db.Decks.Add(deck);
        db.Questions.AddRange(q1, q2);
        await db.SaveChangesAsync(ct);
    }
}
