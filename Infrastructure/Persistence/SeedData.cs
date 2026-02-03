using Domain.Entities;
using Domain.Entities.Deck;

using Infrastructure.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

/// <summary>
///     SeedData: tạo dữ liệu hệ thống tối thiểu để local dev/test chạy ổn định.
///     Nguyên tắc:
///     - Idempotent: chạy nhiều lần không tạo duplicate.
///     - Chỉ seed những thứ nhỏ và an toàn cho môi trường dev.
///     - MVP hiện tại chưa có TestVersion/Deck/ExamRule nên seed theo dạng key-value trong AppSettings.
/// </summary>
public static class SeedData
{
    /// <summary>
    ///     Seed dữ liệu hệ thống tối thiểu.
    /// </summary>
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        // Đảm bảo DB đã được migrate trước khi seed.
        await db.Database.MigrateAsync(ct);

        // Các key này được dùng như "system constants" cho MVP.
        // Khi các bảng domain thật (Deck/Topic/...) được tạo, có thể chuyển seed sang các bảng đó.
        var desired = new Dictionary<string, string>
        {
            ["System.TestVersion"] = "2025.12",
            ["System.DefaultDeck"] = "ALL",
            ["System.DefaultDailyGoalMinutes"] = "15"
        };

        // Lấy tất cả key hiện có một lần để tối ưu.
        List<string>? existingKeys = await db.AppSettings
            .AsNoTracking()
            .Select(x => x.Key)
            .ToListAsync(ct);

        foreach (KeyValuePair<string, string> kv in desired)
        {
            if (existingKeys.Contains(kv.Key))
            {
                continue;
            }

            db.AppSettings.Add(new AppSetting
            {
                Key = kv.Key,
                Value = kv.Value,
                CreatedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync(ct);

        await SeedDecksAsync(db, ct);
    }

    /// <summary>
    ///     Seed dữ liệu dev (system + identity).
    ///     - Idempotent
    ///     - Chỉ chạy khi caller gọi explicit (thường là Development startup)
    /// </summary>
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken ct = default)
    {
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        await SeedAsync(db, ct);
        await SeedAdminAsync(services, configuration, ct);
    }

    private static async Task SeedDecksAsync(AppDbContext db, CancellationToken ct)
    {
        // Nếu đã có deck thì thôi (seed idempotent)
        if (await db.Decks.AnyAsync(ct))
        {
            return;
        }

        var deck = new Deck
        {
            Code = "civics-sample-en",
            Name = "US Civics (Sample)"
        };

        var q1 = new Question
        {
            Deck = deck,
            TestVersion = "2025.12",
            QuestionNo = 1,
            Type = "MCQ",
            PromptEn = "What is the supreme law of the land?",
            PromptVi = "Luật tối cao của quốc gia là gì?",
            ExplainEn = "In the U.S., the Constitution is the supreme law of the land.",
            ExplainVi = "Ở Mỹ, Hiến Pháp là luật tối cao của quốc gia.",
            CorrectOptionKey = "A",
            Options =
            {
                new QuestionOption { Key = "A", TextEn = "The Constitution", TextVi = "Hiến Pháp", SortOrder = 1 },
                new QuestionOption
                {
                    Key = "B", TextEn = "The Declaration of Independence", TextVi = "Tuyên Ngôn Độc Lập", SortOrder = 2
                },
                new QuestionOption
                {
                    Key = "C", TextEn = "The Articles of Confederation", TextVi = "Điều Khoản Liên Bang", SortOrder = 3
                },
                new QuestionOption
                    { Key = "D", TextEn = "The Bill of Rights", TextVi = "Tuyên Ngôn Nhân Quyền", SortOrder = 4 }
            }
        };

        var q2 = new Question
        {
            Deck = deck,
            TestVersion = "2025.12",
            QuestionNo = 2,
            Type = "MCQ",
            PromptEn = "What do we call the first ten amendments to the Constitution?",
            PromptVi = "10 tu chính án đầu tiên của Hiến Pháp được gọi là gì?",
            CorrectOptionKey = "B",
            Options =
            {
                new QuestionOption
                    { Key = "A", TextEn = "The Federalist Papers", TextVi = "Các bài Federalist", SortOrder = 1 },
                new QuestionOption
                    { Key = "B", TextEn = "The Bill of Rights", TextVi = "Tuyên Ngôn Nhân Quyền", SortOrder = 2 },
                new QuestionOption
                {
                    Key = "C", TextEn = "The Emancipation Proclamation", TextVi = "Tuyên Ngôn Giải Phóng", SortOrder = 3
                },
                new QuestionOption { Key = "D", TextEn = "The Magna Carta", TextVi = "Đại Hiến Chương", SortOrder = 4 }
            }
        };

        db.Decks.Add(deck);
        db.Questions.AddRange(q1, q2);

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedAdminAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken ct)
    {
        ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");

        // Always ensure the Admin role exists (safe even without an admin user).
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        const string adminRole = "Admin";

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            var role = new IdentityRole<Guid>(adminRole) { Id = Guid.NewGuid() };
            IdentityResult created = await roleManager.CreateAsync(role);
            if (!created.Succeeded)
            {
                logger.LogWarning(
                    "[DEV] Failed to create role '{Role}': {Errors}",
                    adminRole,
                    string.Join("; ", created.Errors.Select(e => e.Description)));
                return;
            }
        }

        string? email = configuration["Seed:AdminEmail"];
        string? password = configuration["Seed:AdminPassword"];

        // No secrets? Then we only create the role and stop.
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogInformation("[DEV] Admin user seed skipped (Seed:AdminEmail / Seed:AdminPassword not set).");
            return;
        }

        string normalized = email.Trim();

        AppUser? user = await userManager.FindByNameAsync(normalized);
        if (user is null)
        {
            user = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = normalized,
                Email = normalized,
                EmailConfirmed = true
            };

            IdentityResult createUser = await userManager.CreateAsync(user, password);
            if (!createUser.Succeeded)
            {
                logger.LogWarning(
                    "[DEV] Failed to create admin user '{User}': {Errors}",
                    normalized,
                    string.Join("; ", createUser.Errors.Select(e => e.Description)));
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(user, adminRole))
        {
            IdentityResult addRole = await userManager.AddToRoleAsync(user, adminRole);
            if (!addRole.Succeeded)
            {
                logger.LogWarning(
                    "[DEV] Failed to add role '{Role}' to '{User}': {Errors}",
                    adminRole,
                    normalized,
                    string.Join("; ", addRole.Errors.Select(e => e.Description)));
            }
        }
    }
}
