using System.Text.RegularExpressions;

using NUnit.Framework;

using UiE2E.Playwright.Fixtures;

using static Microsoft.Playwright.Assertions;

namespace UiE2E.Playwright.Tests;

[Category("E2E")]
public sealed class SettingsPersistenceTests : E2EPageTest
{
    [Test]
    public async Task Settings_Persist_After_Refresh()
    {
        var seed = await EnsureSeededAsync();

        await LoginAsync(seed.Email, seed.Password);
        await EnsureOnboardedAsync();

        await Page.GotoAsync("/settings");
        await Expect(Page).ToHaveURLAsync(new Regex("/settings$"));
        await Expect(Page.GetByTestId("settings-title")).ToBeVisibleAsync();

        await Page.GetByTestId("settings-system-language").SelectOptionAsync("En");
        await Page.GetByTestId("settings-font-scale").SelectOptionAsync("XLarge");
        await Page.GetByTestId("settings-audio-speed").SelectOptionAsync("Fast");

        await Page.GetByTestId("settings-save").ClickAsync();
        await Expect(Page.GetByTestId("settings-message")).ToBeVisibleAsync();

        await Page.ReloadAsync();

        await Expect(Page.GetByTestId("settings-system-language")).ToHaveValueAsync("En");
        await Expect(Page.GetByTestId("settings-font-scale")).ToHaveValueAsync("XLarge");
        await Expect(Page.GetByTestId("settings-audio-speed")).ToHaveValueAsync("Fast");

        var fontScale = await Page.EvaluateAsync<string>("() => document.documentElement.getAttribute('data-font-scale')");
        Assert.That(fontScale, Is.EqualTo("XLarge"));
    }
}
