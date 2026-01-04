using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

public sealed class TestApiFactory : WebApplicationFactory<Program>
{
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
            // Replace auth with a controllable test scheme.
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });

        return base.CreateHost(builder);
    }
}
