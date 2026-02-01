using Shared.Contracts.Me;

namespace Ui.Blazor.Services;

/// <summary>
/// Client-side cache for the current user's settings.
/// Also applies "global UI effects" (FontScale, etc.) whenever settings change.
/// </summary>
public sealed class UserSettingsState
{
    private readonly ApiClient _api;
    private readonly UiDomInterop _ui;
    private readonly AppState _app;

    public UserSettingsState(ApiClient api, UiDomInterop ui, AppState app)
    {
        _api = api;
        _ui = ui;
        _app = app;
    }

    public UserSettingContracts? Current { get; private set; }

    /// <summary>
    /// Ensures settings are loaded once for the authenticated user.
    /// </summary>
    public async Task EnsureLoadedAsync()
    {
        if (!_app.IsAuthenticated)
        {
            Current = null;
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
            // If API fails, keep Current null and fall back to defaults.
            Current = null;
            await ApplyDefaultsAsync();
        }
    }

    /// <summary>
    /// Force reload from server (use after onboarding complete or save settings).
    /// </summary>
    public async Task ReloadAsync()
    {
        if (!_app.IsAuthenticated)
        {
            Current = null;
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
        // 1) Font scale
        await _ui.SetFontScaleAsync(settings.FontScale.ToString());
    }

    /// <summary>
    /// Apply safe defaults for anonymous/initial state.
    /// </summary>
    public Task ApplyDefaultsAsync()
        => _ui.SetFontScaleAsync("Large").AsTask();

    /// <summary>
    /// Marks cached settings as stale so the next EnsureLoadedAsync will fetch/apply again.
    /// Use this after login/logout/onboarding changes.
    /// </summary>
    public async Task InvalidateAsync()
    {
        Current = null;
        await ApplyDefaultsAsync();
    }
}
