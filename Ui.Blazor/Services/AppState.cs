namespace Ui.Blazor.Services;

public sealed class AppState
{
    private const string AccessTokenKey = "auth.accessToken";
    private const string UserIdKey = "auth.userId";
    private const string IsOnboardedKey = "auth.isOnboarded";

    private readonly StorageInterop _storage;

    public AppState(StorageInterop storage)
    {
        _storage = storage;
    }

    public string? AccessToken { get; private set; }
    public string? UserId { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken);
    public bool IsOnboarded { get; private set; }

    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        AccessToken = await _storage.GetItemAsync(AccessTokenKey);
        UserId = await _storage.GetItemAsync(UserIdKey);

        string? onboardedRaw = await _storage.GetItemAsync(IsOnboardedKey);
        IsOnboarded = string.Equals(onboardedRaw, "true", StringComparison.OrdinalIgnoreCase);

        NotifyStateChanged();
    }

    public async Task SetAuthAsync(string accessToken, string? userId, bool isOnboarded)
    {
        AccessToken = accessToken;
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
        AccessToken = null;
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
