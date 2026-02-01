using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Infrastructure.Auth;

using Domain.Entities.Users;

using FluentAssertions;

using Infrastructure.Identity;
using Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using Shared.Contracts.Auth;

namespace Api.IntegrationTests.Infrastructure;

public static class AuthTestHelper
{
    public static async Task AuthenticateAsync(HttpClient client)
    {
        // Generate unique username/email to avoid collisions across tests/runs.
        var unique = Guid.NewGuid().ToString("N")[..8];
        var username = $"test_{unique}";
        var email = $"{username}@test.local";
        var password = "Test123!@#abc"; // must satisfy Identity password policy

        // 1) Register
        var registerRes = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = email,
            Password = password
        });

        if (registerRes.StatusCode != HttpStatusCode.OK && registerRes.StatusCode != HttpStatusCode.Created)
        {
            var registerBody = await registerRes.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Register failed with {(int)registerRes.StatusCode} {registerRes.StatusCode}. Response: {registerBody}");
        }

        // 2) Login
        var loginRes = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });

        loginRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var login = await loginRes.Content.ReadFromJsonAsync<AuthResponse>();
        login.Should().NotBeNull("login response must contain an access token");
        login.AccessToken.Should().NotBeNullOrWhiteSpace("login response must contain an access token");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.AccessToken);
    }

    public static async Task AuthenticateAsync(HttpClient client, IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var jwt = scope.ServiceProvider.GetRequiredService<JwtTokenService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureCreatedAsync();

        var unique = Guid.NewGuid().ToString("N")[..8];
        var username = $"test_{unique}";
        var email = $"{username}@test.local";
        var password = "Test123!@#abc";

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email
        };

        IdentityResult create = await userManager.CreateAsync(user, password);
        create.Succeeded.Should().BeTrue($"user creation failed: {string.Join("; ", create.Errors.Select(e => e.Description))}");

        DateTime now = DateTime.UtcNow;
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            IsOnboarded = false,
            CreatedUtc = now,
            UpdatedUtc = now
        };

        var settings = new UserSettings
        {
            Id = profile.Id,
            UserId = user.Id,
            CreatedUtc = now,
            UpdatedUtc = now
        };

        db.UserProfiles.Add(profile);
        db.UserSettings.Add(settings);
        await db.SaveChangesAsync();

        IList<string> roles = await userManager.GetRolesAsync(user);
        string token = jwt.CreateAccessToken(user, roles);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task AuthenticateAsAdminAsync(HttpClient client, IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var jwt = scope.ServiceProvider.GetRequiredService<JwtTokenService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureCreatedAsync();

        const string adminRole = "Admin";

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            IdentityResult roleCreate = await roleManager.CreateAsync(new IdentityRole<Guid>(adminRole));
            roleCreate.Succeeded.Should().BeTrue("admin role should be created successfully");
        }

        var unique = Guid.NewGuid().ToString("N")[..8];
        var username = $"admin_{unique}";
        var email = $"{username}@test.local";
        var password = "Test123!@#abc";

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email
        };

        IdentityResult create = await userManager.CreateAsync(user, password);
        create.Succeeded.Should().BeTrue($"user creation failed: {string.Join("; ", create.Errors.Select(e => e.Description))}");

        IdentityResult addRole = await userManager.AddToRoleAsync(user, adminRole);
        addRole.Succeeded.Should().BeTrue($"adding admin role failed: {string.Join("; ", addRole.Errors.Select(e => e.Description))}");

        DateTime now = DateTime.UtcNow;
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            IsOnboarded = false,
            CreatedUtc = now,
            UpdatedUtc = now
        };

        var settings = new UserSettings
        {
            Id = profile.Id,
            UserId = user.Id,
            CreatedUtc = now,
            UpdatedUtc = now
        };

        db.UserProfiles.Add(profile);
        db.UserSettings.Add(settings);
        await db.SaveChangesAsync();

        IList<string> roles = await userManager.GetRolesAsync(user);
        string token = jwt.CreateAccessToken(user, roles);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
}
