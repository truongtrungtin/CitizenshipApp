// ============================================
// UI DOM helpers for Blazor (BL-027)
// Keep this small and focused.
// ============================================
window.__ui = window.__ui || {};

window.__ui.setFontScale = (scale) => {
  // scale is expected to be "Medium", "Large", or "XLarge"
  document.documentElement.setAttribute("data-font-scale", scale || "Medium");
};