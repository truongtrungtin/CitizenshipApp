using Infrastructure.Persistence;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

/// <summary>
/// DependencyInjection:
/// - Gom toàn bộ đăng ký DI của Infrastructure vào 1 nơi.
/// - Api chỉ cần gọi services.AddInfrastructure(configuration);
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Lấy connection string từ Api/appsettings*.json
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Missing ConnectionStrings:DefaultConnection. " +
                "Please set it in Api/appsettings.Development.json");
        }

        // Đăng ký DbContext dùng SQL Server
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(); // optional, ổn hơn khi DB chập chờn
                // sql.CommandTimeout(60);   // optional
            });
        });

        // ---------------------------
        // ASP.NET Core Identity
        // ---------------------------
        // Identity được đặt ở Infrastructure (theo DECISIONS.md)
        // để Domain không phụ thuộc ASP.NET Identity.
        services
            .AddIdentityCore<AppUser>(options =>
            {
                // MVP: đặt rule đơn giản, có thể siết chặt sau.
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        return services;
    }
}
