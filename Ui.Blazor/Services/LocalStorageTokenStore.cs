using Microsoft.JSInterop;

namespace Ui.Blazor.Services;

public sealed class LocalStorageTokenStore : ITokenStore
{
    private const string AccessTokenKey = "auth.accessToken";
    private readonly IJSRuntime _js;

    public LocalStorageTokenStore(IJSRuntime js)
    {
        _js = js;
    }

    public async ValueTask<string?> GetAccessTokenAsync()
    {
        return await _js.InvokeAsync<string?>("appStorage.get", AccessTokenKey);
    }

    public async ValueTask SetAccessTokenAsync(string token)
    {
        await _js.InvokeVoidAsync("appStorage.set", AccessTokenKey, token);
    }

    public async ValueTask ClearAsync()
    {
        await _js.InvokeVoidAsync("appStorage.remove", AccessTokenKey);
    }
}
