using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.Playwright;

using NUnit.Framework;
using NUnit.Framework.Interfaces;

using Shared.Contracts.E2E;

using static Microsoft.Playwright.Assertions;

namespace UiE2E.Playwright.Fixtures;

public abstract class E2EPageTest
{
    protected static readonly E2EConfig Config = E2EConfig.FromEnvironment();
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private string ArtifactsRoot => Path.Combine(TestContext.CurrentContext.WorkDirectory, "e2e-artifacts");

    protected IPlaywright Playwright { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = Config.Headless,
            SlowMo = Config.SlowMoMs > 0 ? Config.SlowMoMs : null
        });

        Context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = Config.BaseUrl,
            IgnoreHTTPSErrors = Config.IgnoreHttpsErrors,
            RecordVideoDir = Config.RecordVideo ? EnsureDir("videos") : null
        });

        Page = await Context.NewPageAsync();
        Page.SetDefaultTimeout(15000);
        Context.SetDefaultTimeout(15000);

        if (Config.RecordTrace)
        {
            await Context.Tracing.StartAsync(new TracingStartOptions
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });
        }
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        var status = TestContext.CurrentContext.Result.Outcome.Status;

        if (status == TestStatus.Failed && Page is not null)
        {
            var screenshotPath = Path.Combine(EnsureDir("screenshots"), SanitizeFileName(TestContext.CurrentContext.Test.Name) + ".png");
            await Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath, FullPage = true });
            TestContext.AddTestAttachment(screenshotPath);
        }

        if (Config.RecordTrace && Context is not null)
        {
            if (status == TestStatus.Failed)
            {
                var tracePath = Path.Combine(EnsureDir("traces"), SanitizeFileName(TestContext.CurrentContext.Test.Name) + ".zip");
                await Context.Tracing.StopAsync(new TracingStopOptions { Path = tracePath });
                TestContext.AddTestAttachment(tracePath);
            }
            else
            {
                await Context.Tracing.StopAsync();
            }
        }

        if (Context is not null)
        {
            await Context.CloseAsync();
        }

        if (Browser is not null)
        {
            await Browser.CloseAsync();
        }

        Playwright?.Dispose();
    }

    protected async Task<E2ESeedResponse> EnsureSeededAsync()
    {
        var apiContext = await Playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = Config.ApiBaseUrl,
            IgnoreHTTPSErrors = Config.IgnoreHttpsErrors
        });

        try
        {
            var response = await apiContext.PostAsync("/api/e2e/seed");
            if (!response.Ok)
            {
                var body = await response.TextAsync();
                throw new InvalidOperationException($"Seed failed: {response.Status} {response.StatusText}\n{body}");
            }

            var json = await response.TextAsync();
            var payload = JsonSerializer.Deserialize<E2ESeedResponse>(json, JsonOptions);
            return payload ?? throw new InvalidOperationException("Seed response was empty.");
        }
        finally
        {
            await apiContext.DisposeAsync();
        }
    }

    protected async Task LoginAsync(string email, string password)
    {
        await Page.GotoAsync("/login");
        await Page.GetByTestId("login-username").FillAsync(email);
        await Page.GetByTestId("login-password").FillAsync(password);
        await Page.GetByTestId("login-submit").ClickAsync();
        await Page.WaitForURLAsync(new Regex("/(home|onboarding)$"));
    }

    protected async Task EnsureOnboardedAsync()
    {
        if (Page.Url.Contains("/onboarding", StringComparison.OrdinalIgnoreCase))
        {
            await Page.GetByTestId("onboarding-start").ClickAsync();
            await Expect(Page).ToHaveURLAsync(new Regex("/home$"));
        }
    }

    protected static string NewEmail() => $"e2e_{Guid.NewGuid():N}@example.com";

    protected static string DefaultPassword => "P@ssw0rd!123";

    private string EnsureDir(string name)
    {
        var dir = Path.Combine(ArtifactsRoot, name);
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }

        return name;
    }
}
