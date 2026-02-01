using Domain.Enums;

using Microsoft.JSInterop;

namespace Ui.Blazor.Services;

public sealed class TtsService
{
    private readonly IJSRuntime _js;

    public TtsService(IJSRuntime js) => _js = js;

    public sealed record TtsVoice(string Name, string Lang, bool IsDefault);

    public ValueTask<bool> IsSupportedAsync()
        => _js.InvokeAsync<bool>("__tts.isSupported");

    public ValueTask CancelAsync()
        => _js.InvokeVoidAsync("__tts.cancel");

    public ValueTask<IReadOnlyList<TtsVoice>> GetEnglishVoicesAsync()
        => GetVoicesByLangAsync("en");

    public async ValueTask<IReadOnlyList<TtsVoice>> GetVoicesByLangAsync(string langPrefix)
    {
        var voices = await _js.InvokeAsync<TtsVoice[]>("__tts.getVoices");
        return voices.Where(v => v.Lang.StartsWith(langPrefix, StringComparison.OrdinalIgnoreCase)).ToArray();
    }

    public ValueTask SpeakAsync(string text, LanguageCode language, AudioSpeed speed, string? voiceName = null)
    {
        if (language == LanguageCode.Vi)
        {
            return ValueTask.CompletedTask;
        }

        string lang = "en-US";
        double rate = speed switch
        {
            AudioSpeed.Slow => 0.8,
            AudioSpeed.Normal => 1.0,
            AudioSpeed.Fast => 1.2,
            _ => 1.0
        };

        return _js.InvokeVoidAsync("__tts.speak", text, lang, rate, voiceName, null);
    }

    public ValueTask SpeakAsync<T>(string text, LanguageCode language, AudioSpeed speed, DotNetObjectReference<T> dotnetRef, string? voiceName = null)
        where T : class
    {
        if (language == LanguageCode.Vi)
        {
            return ValueTask.CompletedTask;
        }

        string lang = "en-US";
        double rate = speed switch
        {
            AudioSpeed.Slow => 0.8,
            AudioSpeed.Normal => 1.0,
            AudioSpeed.Fast => 1.2,
            _ => 1.0
        };

        return _js.InvokeVoidAsync("__tts.speak", text, lang, rate, voiceName, dotnetRef);
    }
}
