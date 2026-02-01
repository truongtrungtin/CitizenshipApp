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

window.__tts.speak = (text, lang, rate, voiceName, dotnetRef) => {
  if (!window.__tts.isSupported()) return;
  if (!text || !text.trim()) return;

  // Always cancel before speaking to avoid overlap
  window.speechSynthesis.cancel();

  const utterance = new SpeechSynthesisUtterance(text);
  if (lang) utterance.lang = lang;
  if (rate) utterance.rate = rate;

  if (voiceName) {
    const voices = window.speechSynthesis.getVoices();
    const match = voices.find(v => v.name === voiceName);
    if (match) utterance.voice = match;
  }

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

window.__tts.getVoices = () => {
  if (!window.__tts.isSupported()) return [];

  const mapVoices = (voices) =>
    voices.map(v => ({ name: v.name, lang: v.lang, isDefault: v.default || false }));

  const voices = window.speechSynthesis.getVoices();
  if (voices && voices.length) return mapVoices(voices);

  return new Promise((resolve) => {
    const handler = () => {
      const list = window.speechSynthesis.getVoices();
      resolve(mapVoices(list || []));
      window.speechSynthesis.removeEventListener('voiceschanged', handler);
    };

    window.speechSynthesis.addEventListener('voiceschanged', handler);

    setTimeout(() => {
      const list = window.speechSynthesis.getVoices();
      resolve(mapVoices(list || []));
      window.speechSynthesis.removeEventListener('voiceschanged', handler);
    }, 500);
  });
};
