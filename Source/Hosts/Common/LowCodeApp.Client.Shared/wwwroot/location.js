// WebDeviceLocation 用。navigator.geolocation を Promise にするだけ。失敗は null。
export function getPosition(timeoutMs) {
  return new Promise((resolve) => {
    try {
      if (!navigator.geolocation) { resolve(null); return; }
      navigator.geolocation.getCurrentPosition(
        (p) => resolve({ latitude: p.coords.latitude, longitude: p.coords.longitude, accuracy: p.coords.accuracy }),
        () => resolve(null),
        { enableHighAccuracy: true, timeout: timeoutMs, maximumAge: 0 });
    } catch { resolve(null); }
  });
}
