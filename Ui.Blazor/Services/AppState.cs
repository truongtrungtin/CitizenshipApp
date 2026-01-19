namespace Ui.Blazor.Services;

public sealed class AppState
{
    private const string AccessTokenKey = "auth.accessToken";
    private const string UserIdKey = "auth.userId";
    private const string IsOnboardedKey = "auth.isOnboarded";

    private readonly StorageInterop _storage;
    private readonly AuthSession _session;

    public AppState(StorageInterop storage, AuthSession session)
    {
        _storage = storage;
        _session = session;
    }

    // Source of truth for token is AuthSession (singleton)
    public string? AccessToken => _session.AccessToken;

    public string? UserId { get; private set; }

    public bool IsAuthenticated => _session.IsAuthenticated;

    public bool IsOnboarded { get; private set; }

    public event Action? OnChange;
    public bool IsInitialized { get; private set; }


    public async Task InitializeAsync()
    {
        var token = await _storage.GetItemAsync(AccessTokenKey);
        _session.SetToken(string.IsNullOrWhiteSpace(token) ? null : token);

        UserId = await _storage.GetItemAsync(UserIdKey);

        string? onboardedRaw = await _storage.GetItemAsync(IsOnboardedKey);
        IsOnboarded = string.Equals(onboardedRaw, "true", StringComparison.OrdinalIgnoreCase);

        IsInitialized = true;

        NotifyStateChanged();
    }

    public async Task SetAuthAsync(string accessToken, string? userId, bool isOnboarded)
    {
        _session.SetToken(accessToken);

        UserId = userId;
        IsOnboarded = isOnboarded;

        await _storage.SetItemAsync(AccessTokenKey, accessToken);
        await _storage.SetItemAsync(UserIdKey, userId ?? "");
        await _storage.SetItemAsync(IsOnboardedKey, isOnboarded ? "true" : "false");

        NotifyStateChanged();
    }

    public async Task SetOnboardedAsync(bool isOnboarded)
    {
        IsOnboarded = isOnboarded;
        await _storage.SetItemAsync(IsOnboardedKey, isOnboarded ? "true" : "false");
        NotifyStateChanged();
    }

    public async Task LogoutAsync()
    {
        _session.Clear();

        UserId = null;
        IsOnboarded = false;

        await _storage.RemoveItemAsync(AccessTokenKey);
        await _storage.RemoveItemAsync(UserIdKey);
        await _storage.RemoveItemAsync(IsOnboardedKey);

        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }
}
