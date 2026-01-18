namespace Ui.Blazor.Services;

/// <summary>
/// Singleton auth session shared across DI scopes (UI + HttpClient handlers).
/// Stores only the current access token in-memory.
/// </summary>
public sealed class AuthSession
{
    public string? AccessToken { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken);

    public void SetToken(string? token) => AccessToken = token;

    public void Clear() => AccessToken = null;
}