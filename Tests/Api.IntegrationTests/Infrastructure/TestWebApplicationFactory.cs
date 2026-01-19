using System.Collections.Generic;
using System;
using System.Data.Common;
using System.Linq;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using Infrastructure.Persistence; // AppDbContext namespace của bạn

namespace Api.IntegrationTests.Infrastructure;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{

    private DbConnection? _connection;
    private ServiceProvider? _sqliteProvider;
    private static readonly object DbInitLock = new();
    private static bool _dbInitialized;
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            "Data Source=:memory:");
        Environment.SetEnvironmentVariable(
            "Jwt__Key",
            "test-jwt-key-32-characters-long!!");
        Environment.SetEnvironmentVariable(
            "Jwt__Issuer",
            "TestIssuer");
        Environment.SetEnvironmentVariable(
            "Jwt__Audience",
            "TestAudience");

        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                ["Jwt:Key"] = "test-jwt-key-32-characters-long!!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            };

            config.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            // ------------------------------------------------------------
            // 1) Remove existing EF registrations (DbContext + options)
            // ------------------------------------------------------------
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<IDbContextFactory<AppDbContext>>();
            services.RemoveAll<IConfigureOptions<DbContextOptions<AppDbContext>>>();
            services.RemoveAll<IConfigureNamedOptions<DbContextOptions<AppDbContext>>>();

            var dbContextOptionsConfigurations = services
                .Where(descriptor =>
                    descriptor.ServiceType.FullName?.Contains("IDbContextOptionsConfiguration") == true &&
                    descriptor.ServiceType.GenericTypeArguments.FirstOrDefault() == typeof(AppDbContext))
                .ToList();

            foreach (var descriptor in dbContextOptionsConfigurations)
            {
                services.Remove(descriptor);
            }

            // ------------------------------------------------------------
            // 2) Configure SQLite in-memory
            // ------------------------------------------------------------
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            _sqliteProvider ??= new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .BuildServiceProvider();

            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Warning);
            });

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning));

                // Use a single isolated provider so the app service provider can
                // keep its SqlServer services without conflicts.
                options.UseInternalServiceProvider(_sqliteProvider);

                // IMPORTANT:
                // UseInternalServiceProvider is required to avoid provider conflicts
                // when the app service provider still has SqlServer registrations.
            });

            // ------------------------------------------------------------
            // 3) Initialize schema
            // ------------------------------------------------------------
            lock (DbInitLock)
            {
                if (_dbInitialized) return;
                _dbInitialized = true;

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
        if (_sqliteProvider is IDisposable disposableProvider)
        {
            disposableProvider.Dispose();
        }
    }
}
