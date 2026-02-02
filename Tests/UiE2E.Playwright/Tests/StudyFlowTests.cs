using System.Text.RegularExpressions;

using NUnit.Framework;

using UiE2E.Playwright.Fixtures;

using static Microsoft.Playwright.Assertions;

namespace UiE2E.Playwright.Tests;

[Category("E2E")]
public sealed class StudyFlowTests : E2EPageTest
{
    [Test]
    public async Task Study_Next_Answer_Progress_Works()
    {
        var seed = await EnsureSeededAsync();

        await LoginAsync(seed.Email, seed.Password);
        await EnsureOnboardedAsync();

        await Page.GotoAsync("/study");
        await Expect(Page).ToHaveURLAsync(new Regex("/study$"));

        var question = Page.GetByTestId("study-question");
        await Expect(question).ToBeVisibleAsync();

        var firstAnswer = Page.Locator("[data-testid^='study-answer-']").First;
        await Expect(firstAnswer).ToBeVisibleAsync();
        await firstAnswer.ClickAsync();

        await Expect(Page.GetByTestId("study-result-badge")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("study-continue")).ToBeVisibleAsync();

        await Page.GetByTestId("study-continue").ClickAsync();
        await Expect(question).ToBeVisibleAsync();

        var answeredRaw = await Page.GetByTestId("study-progress-answered").InnerTextAsync();
        _ = int.TryParse(answeredRaw, out var answered);
        Assert.That(answered, Is.GreaterThanOrEqualTo(1));
    }
}
