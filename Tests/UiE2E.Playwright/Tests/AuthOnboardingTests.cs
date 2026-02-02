using System.Text.RegularExpressions;

using NUnit.Framework;

using UiE2E.Playwright.Fixtures;

using static Microsoft.Playwright.Assertions;

namespace UiE2E.Playwright.Tests;

public sealed class AuthOnboardingTests : E2EPageTest
{
    [Test]
    public async Task Register_Login_Onboarding_Guards_Work()
    {
        var email = NewEmail();
        var password = DefaultPassword;

        await Page.GotoAsync("/register");
        await Page.GetByTestId("register-username").FillAsync(email);
        await Page.GetByTestId("register-password").FillAsync(password);
        await Page.GetByTestId("register-submit").ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/onboarding$"));
        await Expect(Page.GetByTestId("onboarding-title")).ToBeVisibleAsync();

        var token = await Page.EvaluateAsync<string>("() => localStorage.getItem('auth.accessToken')");
        Assert.That(token, Is.Not.Null.And.Not.Empty);

        await Page.GotoAsync("/logout");
        await Expect(Page).ToHaveURLAsync(new Regex("/login$"));

        await Page.GotoAsync("/study");
        await Expect(Page).ToHaveURLAsync(new Regex("/login$"));

        await Page.GetByTestId("login-username").FillAsync(email);
        await Page.GetByTestId("login-password").FillAsync(password);
        await Page.GetByTestId("login-submit").ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/onboarding$"));
        await Page.GetByTestId("onboarding-start").ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/home$"));

        await Page.GotoAsync("/study");
        await Expect(Page).ToHaveURLAsync(new Regex("/study$"));
        await Expect(Page.GetByTestId("study-title")).ToBeVisibleAsync();
    }
}
