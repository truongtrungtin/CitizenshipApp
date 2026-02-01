// ============================================
// Web Speech API helpers for Blazor (TTS)
// ============================================
window.__tts = window.__tts || {};

window.__tts.isSupported = () => {
  return typeof window !== "undefined" && "speechSynthesis" in window;
};

window.__tts.cancel = () => {
  if (window.__tts.isSupported()) {
    window.speechSynthesis.cancel();
  }
};

window.__tts.speak = (text, lang, rate, dotnetRef) => {
  if (!window.__tts.isSupported()) return;
  if (!text || !text.trim()) return;

  // Always cancel before speaking to avoid overlap
  window.speechSynthesis.cancel();

  const utterance = new SpeechSynthesisUtterance(text);
  if (lang) utterance.lang = lang;
  if (rate) utterance.rate = rate;

  utterance.onend = () => {
    if (dotnetRef && dotnetRef.invokeMethodAsync) {
      dotnetRef.invokeMethodAsync("OnSpeakEnded");
    }
  };

  utterance.onerror = () => {
    if (dotnetRef && dotnetRef.invokeMethodAsync) {
      dotnetRef.invokeMethodAsync("OnSpeakEnded");
    }
  };

  window.speechSynthesis.speak(utterance);
};
