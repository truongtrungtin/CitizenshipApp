using Domain.Entities;
using Domain.Entities.Users;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext của hệ thống.
///
/// IMPORTANT:
/// - DbContext này kế thừa <see cref="IdentityDbContext"/> để có sẵn bảng Identity
///   (AspNetUsers, AspNetRoles, ...).
/// - Domain entities (UserProfile/UserSettings/AppSetting) vẫn nằm trong Domain.
/// - Mapping/Indexes/Relationships được cấu hình bằng Fluent API.
/// </summary>
public sealed class AppDbContext
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

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
    }
}
