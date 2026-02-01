using Domain.Enums;

using Shared.Contracts.Me;

namespace Ui.Blazor.Services;

/// <summary>
/// Client-side cache for the current user's settings.
/// Also applies "global UI effects" (FontScale, etc.) whenever settings change.
/// </summary>
public sealed class UserSettingsState
{
    private const string SystemLanguageKey = "ui.systemLanguage";

    private readonly ApiClient _api;
    private readonly UiDomInterop _ui;
    private readonly AppState _app;
    private readonly StorageInterop _storage;

    public UserSettingsState(ApiClient api, UiDomInterop ui, AppState app, StorageInterop storage)
    {
        _api = api;
        _ui = ui;
        _app = app;
        _storage = storage;
    }

    public UserSettingContracts? Current { get; private set; }

    public event Action? OnChange;

    /// <summary>
    /// Ensures settings are loaded once for the authenticated user.
    /// </summary>
    public async Task EnsureLoadedAsync()
    {
        await ApplySystemLanguageFallbackAsync();

        if (!_app.IsAuthenticated)
        {
            Current ??= new UserSettingContracts();
            await ApplyDefaultsAsync();
            return;
        }

        if (Current is not null) return;

        try
        {
            Current = await _api.GetMeSettingsFullAsync();
            await ApplyAsync(Current);
        }
        catch
        {
            // If API fails, keep local fallback and apply defaults.
            Current ??= new UserSettingContracts();
            await ApplyDefaultsAsync();
        }
    }

    /// <summary>
    /// Force reload from server (use after onboarding complete or save settings).
    /// </summary>
    public async Task ReloadAsync()
    {
        await ApplySystemLanguageFallbackAsync();

        if (!_app.IsAuthenticated)
        {
            Current ??= new UserSettingContracts();
            await ApplyDefaultsAsync();
            return;
        }

        Current = await _api.GetMeSettingsFullAsync();
        await ApplyAsync(Current);
    }

    /// <summary>
    /// Apply UI effects for the given settings (no API call).
    /// </summary>
    public async Task ApplyAsync(UserSettingContracts settings)
    {
        Current = settings;

        await _storage.SetItemAsync(SystemLanguageKey, settings.SystemLanguage.ToString());

        // 1) Font scale
        await _ui.SetFontScaleAsync(settings.FontScale.ToString());
        NotifyChanged();
    }

    /// <summary>
    /// Preview settings locally (no API call). Useful for instant UI language changes.
    /// </summary>
    public Task PreviewAsync(UserSettingContracts settings)
    {
        return ApplyAsync(settings);
    }

    /// <summary>
    /// Apply safe defaults for anonymous/initial state.
    /// </summary>
    public Task ApplyDefaultsAsync()
    {
        return ApplyDefaultsInternalAsync();
    }

    /// <summary>
    /// Marks cached settings as stale so the next EnsureLoadedAsync will fetch/apply again.
    /// Use this after login/logout/onboarding changes.
    /// </summary>
    public async Task InvalidateAsync()
    {
        Current = null;
        await ApplyDefaultsAsync();
    }

    private async Task ApplyDefaultsInternalAsync()
    {
        await ApplySystemLanguageFallbackAsync();
        NotifyChanged();
        await _ui.SetFontScaleAsync("Large");
    }

    private async Task ApplySystemLanguageFallbackAsync()
    {
        if (Current is not null)
            return;

        string? raw = await _storage.GetItemAsync(SystemLanguageKey);
        if (string.IsNullOrWhiteSpace(raw))
            return;

        if (!Enum.TryParse<LanguageCode>(raw, true, out var lang))
            return;

        Current = new UserSettingContracts { SystemLanguage = lang };
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        OnChange?.Invoke();
    }
}
