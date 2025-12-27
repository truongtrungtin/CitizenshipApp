using Microsoft.JSInterop;

namespace Ui.Blazor.Services;

public sealed class StorageInterop
{
    private readonly IJSRuntime _js;

    public StorageInterop(IJSRuntime js) => _js = js;

    public ValueTask<string?> GetItemAsync(string key)
        => _js.InvokeAsync<string?>("window.__storage.getItem", key);

    public ValueTask SetItemAsync(string key, string value)
        => _js.InvokeVoidAsync("window.__storage.setItem", key, value);

    public ValueTask RemoveItemAsync(string key)
        => _js.InvokeVoidAsync("window.__storage.removeItem", key);
}
