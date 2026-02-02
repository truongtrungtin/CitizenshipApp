using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

using UiE2E.Playwright.Fixtures;

namespace UiE2E.Playwright.Tests;

public sealed class NegativeCasesTests : E2EPageTest
{
    [Test]
    public async Task Login_WrongPassword_ShowsError()
    {
        var seed = await EnsureSeededAsync();

        await Page.GotoAsync("/login");
        await Page.GetByTestId("login-username").FillAsync(seed.Email);
        await Page.GetByTestId("login-password").FillAsync("wrong-password");
        await Page.GetByTestId("login-submit").ClickAsync();

        await Expect(Page.GetByTestId("login-error")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Study_WhenApiFails_ShowsErrorState()
    {
        var seed = await EnsureSeededAsync();

        await LoginAsync(seed.Email, seed.Password);
        await EnsureOnboardedAsync();

        await Page.RouteAsync("**/api/Study/next**", async route =>
        {
            var problem = JsonSerializer.Serialize(new
            {
                title = "Internal Server Error",
                detail = "E2E forced error"
            });

            await route.FulfillAsync(new RouteFulfillOptions
            {
                Status = 500,
                ContentType = "application/problem+json",
                Body = problem
            });
        });

        await Page.GotoAsync("/study");
        await Expect(Page).ToHaveURLAsync(new Regex("/study$"));

        await Page.WaitForResponseAsync(r =>
            r.Url.Contains("/api/study/next", StringComparison.OrdinalIgnoreCase) && r.Status == 500);

        await Expect(Page.GetByTestId("study-error")).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions
        {
            Timeout = 15000
        });
    }
}
