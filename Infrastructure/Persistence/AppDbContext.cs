using Domain.Entities;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // UserProfile
        modelBuilder.Entity<UserProfile>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedNever(); // nếu bạn dùng Guid tự tạo
        });

        // UserSettings (1-1 với UserProfile)
        modelBuilder.Entity<UserSettings>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<UserProfile>()
            .HasOne<UserSettings>()
            .WithOne()
            .HasForeignKey<UserSettings>(x => x.Id); // dùng chung Id làm FK (tuỳ entity bạn đang thiết kế)

        modelBuilder.Entity<AppSetting>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Key).HasMaxLength(200).IsRequired();
            b.Property(x => x.Value).HasMaxLength(2000).IsRequired();
            b.HasIndex(x => x.Key).IsUnique();
        });
    }
}
