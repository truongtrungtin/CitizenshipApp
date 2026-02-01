using Domain.Enums;

using Microsoft.JSInterop;

namespace Ui.Blazor.Services;

public sealed class TtsService
{
    private readonly IJSRuntime _js;

    public TtsService(IJSRuntime js) => _js = js;

    public ValueTask<bool> IsSupportedAsync()
        => _js.InvokeAsync<bool>("__tts.isSupported");

    public ValueTask CancelAsync()
        => _js.InvokeVoidAsync("__tts.cancel");

    public ValueTask SpeakAsync(string text, LanguageCode language, AudioSpeed speed)
    {
        string lang = language == LanguageCode.Vi ? "vi-VN" : "en-US";
        double rate = speed switch
        {
            AudioSpeed.Slow => 0.8,
            AudioSpeed.Normal => 1.0,
            AudioSpeed.Fast => 1.2,
            _ => 1.0
        };

        return _js.InvokeVoidAsync("__tts.speak", text, lang, rate, null);
    }

    public ValueTask SpeakAsync<T>(string text, LanguageCode language, AudioSpeed speed, DotNetObjectReference<T> dotnetRef)
        where T : class
    {
        string lang = language == LanguageCode.Vi ? "vi-VN" : "en-US";
        double rate = speed switch
        {
            AudioSpeed.Slow => 0.8,
            AudioSpeed.Normal => 1.0,
            AudioSpeed.Fast => 1.2,
            _ => 1.0
        };

        return _js.InvokeVoidAsync("__tts.speak", text, lang, rate, dotnetRef);
    }
}
