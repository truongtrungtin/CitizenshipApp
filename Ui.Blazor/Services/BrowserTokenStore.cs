using Microsoft.JSInterop;

namespace Ui.Blazor.Services;

public sealed class BrowserTokenStore : ITokenStore
{
    private const string Key = "auth.accessToken";
    private readonly IJSRuntime _js;

    public BrowserTokenStore(IJSRuntime js)
    {
        _js = js;
    }

    public ValueTask<string?> GetAccessTokenAsync()
    {
        return _js.InvokeAsync<string?>("window.__storage.getItem", Key);
    }

    public ValueTask SetAccessTokenAsync(string token)
    {
        return _js.InvokeVoidAsync("window.__storage.setItem", Key, token);
    }

    public ValueTask ClearAsync()
    {
        return _js.InvokeVoidAsync("window.__storage.removeItem", Key);
    }
}
