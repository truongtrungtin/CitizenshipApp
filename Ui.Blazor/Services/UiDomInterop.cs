using Microsoft.JSInterop;

namespace Ui.Blazor.Services;

/// <summary>
/// Typed wrapper around small JS DOM helpers (wwwroot/js/ui.js).
/// Keeps JS calls centralized and easy to test/mock.
/// </summary>
public sealed class UiDomInterop
{
    private readonly IJSRuntime _js;

    public UiDomInterop(IJSRuntime js) => _js = js;

    /// <summary>
    /// Applies the global font scale by setting an attribute on the root HTML element.
    /// CSS then scales the entire app via rem units.
    /// </summary>
    public ValueTask SetFontScaleAsync(string scale)
        => _js.InvokeVoidAsync("__ui.setFontScale", scale);
}
