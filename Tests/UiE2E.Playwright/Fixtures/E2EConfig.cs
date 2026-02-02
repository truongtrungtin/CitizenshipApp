namespace UiE2E.Playwright.Fixtures;

public sealed class E2EConfig
{
    public string BaseUrl { get; init; } = "https://localhost:7031";
    public string ApiBaseUrl { get; init; } = "https://localhost:7070";
    public bool Headless { get; init; } = true;
    public bool IgnoreHttpsErrors { get; init; } = true;
    public bool RecordVideo { get; init; }
    public bool RecordTrace { get; init; } = true;
    public int SlowMoMs { get; init; }

    public static E2EConfig FromEnvironment()
    {
        return new E2EConfig
        {
            BaseUrl = GetEnv("E2E_BASE_URL", "https://localhost:7031"),
            ApiBaseUrl = GetEnv("E2E_API_BASE_URL", "https://localhost:7070"),
            Headless = !IsFalse("E2E_HEADLESS"),
            IgnoreHttpsErrors = !IsFalse("E2E_IGNORE_HTTPS_ERRORS"),
            RecordVideo = IsTrue("E2E_RECORD_VIDEO"),
            RecordTrace = !IsFalse("E2E_RECORD_TRACE"),
            SlowMoMs = GetInt("E2E_SLOWMO")
        };
    }

    private static string GetEnv(string key, string fallback)
        => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key))
            ? fallback
            : Environment.GetEnvironmentVariable(key)!.Trim();

    private static bool IsTrue(string key)
    {
        var raw = Environment.GetEnvironmentVariable(key);
        return raw is not null && (raw.Equals("1") || raw.Equals("true", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsFalse(string key)
    {
        var raw = Environment.GetEnvironmentVariable(key);
        return raw is not null && (raw.Equals("0") || raw.Equals("false", StringComparison.OrdinalIgnoreCase));
    }

    private static int GetInt(string key)
        => int.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : 0;
}
