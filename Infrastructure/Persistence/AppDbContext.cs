using Domain.Entities;
using Domain.Entities.Deck;
using Domain.Entities.Users;
using Domain.Enums;

using Infrastructure.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
///     EF Core DbContext của hệ thống.
///     IMPORTANT:
///     - DbContext này kế thừa <see cref="IdentityDbContext" /> để có sẵn bảng Identity
///     (AspNetUsers, AspNetRoles, ...).
///     - Domain entities (UserProfile/UserSettings/AppSetting) vẫn nằm trong Domain.
///     - Mapping/Indexes/Relationships được cấu hình bằng Fluent API.
/// </summary>
public sealed class AppDbContext
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<StudyEvent> StudyEvents => Set<StudyEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------------------------
        // DOMAIN TABLES
        // ---------------------------

        // UserProfile
        modelBuilder.Entity<UserProfile>(b =>
        {
            b.ToTable("UserProfiles");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedNever();

            // 1 user có đúng 1 profile (enforced bằng unique index)
            b.HasIndex(x => x.UserId).IsUnique();

            // FK từ UserProfiles.UserId -> AspNetUsers.Id
            b.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Property(x => x.IsOnboarded).HasDefaultValue(false);
            b.Property(x => x.CreatedUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            b.Property(x => x.UpdatedUtc).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // UserSettings
        modelBuilder.Entity<UserSettings>(b =>
        {
            b.ToTable("UserSettings");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedNever();

            // 1 user có đúng 1 settings (enforced bằng unique index)
            b.HasIndex(x => x.UserId).IsUnique();

            // 1-1: Settings thuộc về 1 Profile.
            // Pattern: shared primary key (Settings.Id == Profile.Id).
            b.HasOne<UserProfile>()
                .WithOne()
                .HasForeignKey<UserSettings>(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);

            b.Property(x => x.DailyGoalMinutes).HasDefaultValue(15);
            b.Property(x => x.Language).HasDefaultValue(LanguageCode.En);
            b.Property(x => x.SystemLanguage).HasDefaultValue(LanguageCode.Vi);
            b.Property(x => x.Voice).HasMaxLength(200).HasDefaultValue("");
            b.Property(x => x.SilentMode).HasDefaultValue(false);
            b.Property(x => x.CreatedUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            b.Property(x => x.UpdatedUtc).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // AppSetting
        modelBuilder.Entity<AppSetting>(b =>
        {
            b.ToTable("AppSettings");
            b.HasKey(x => x.Id);
            b.Property(x => x.Key).HasMaxLength(200).IsRequired();
            b.Property(x => x.Value).HasMaxLength(2000).IsRequired();
            b.HasIndex(x => x.Key).IsUnique();
            b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // =========================
        // Deck / Question / Study
        // =========================

        modelBuilder.Entity<Deck>(b =>
        {
            b.ToTable("Decks");
            b.HasKey(x => x.DeckId);

            b.Property(x => x.Code).HasMaxLength(100).IsRequired();
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();

            b.HasIndex(x => x.Code).IsUnique();

            b.HasMany(x => x.Questions)
                .WithOne(x => x.Deck)
                .HasForeignKey(x => x.DeckId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(b =>
        {
            b.ToTable("Questions");
            b.HasKey(x => x.QuestionId);

            b.Property(x => x.Type).HasMaxLength(50).IsRequired();

            b.Property(x => x.TestVersion).HasMaxLength(50).IsRequired();
            b.Property(x => x.QuestionNo).IsRequired();

            b.Property(x => x.PromptEn).HasMaxLength(2000).IsRequired();
            b.Property(x => x.PromptVi).HasMaxLength(2000);
            b.Property(x => x.PromptViPhonetic).HasMaxLength(2000);

            b.Property(x => x.ExplainEn).HasMaxLength(4000);
            b.Property(x => x.ExplainVi).HasMaxLength(4000);

            b.Property(x => x.CorrectOptionKey).HasMaxLength(10).IsRequired();

            b.HasMany(x => x.Options)
                .WithOne(x => x.Question)
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.DeckId);
            b.HasIndex(x => new { x.DeckId, x.QuestionId });
            b.HasIndex(x => new { x.TestVersion, x.QuestionNo }).IsUnique();
        });

        modelBuilder.Entity<QuestionOption>(b =>
        {
            b.ToTable("QuestionOptions");

            // Composite key: (QuestionId + Key)
            b.HasKey(x => new { x.QuestionId, x.Key });

            b.Property(x => x.Key).HasMaxLength(10).IsRequired();
            b.Property(x => x.TextEn).HasMaxLength(500).IsRequired();
            b.Property(x => x.TextVi).HasMaxLength(500);

            b.HasIndex(x => new { x.QuestionId, x.SortOrder });
        });

        modelBuilder.Entity<StudyEvent>(b =>
        {
            b.ToTable("StudyEvents");
            b.HasKey(x => x.StudyEventId);

            b.Property(x => x.SelectedKey).HasMaxLength(10).IsRequired();

            b.HasIndex(x => new { x.UserId, x.CreatedUtc });
            b.HasIndex(x => new { x.DeckId, x.CreatedUtc });
        });
    }
}
