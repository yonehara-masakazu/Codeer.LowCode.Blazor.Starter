// Haptics サービス用。navigator.vibrate と localStorage を包むだけ。すべて失敗しても投げない。
const patterns = {
  Success: 40,
  Warning: [40, 60, 40],
  Error: [60, 60, 60, 60, 200], // Warning と区別できるよう最後を長く
  Selection: 10,
  Impact: 25,
};

export function isSupported() {
  try { return typeof navigator !== 'undefined' && typeof navigator.vibrate === 'function'; } catch { return false; }
}

export function vibrate(kind) {
  try {
    if (isSupported() && patterns[kind] !== undefined) navigator.vibrate(patterns[kind]);
  } catch { }
}

export function loadSetting(key) {
  try { return window.localStorage.getItem(key); } catch { return null; }
}

export function saveSetting(key, value) {
  try { window.localStorage.setItem(key, value); } catch { }
}
