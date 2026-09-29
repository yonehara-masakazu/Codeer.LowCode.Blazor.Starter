// CameraFieldComponent の Web 用。input[type=file] の選択ファイルを縮小 JPEG (base64) にして .NET へ返す。
const states = new WeakMap();

export function init(camera, library, dotNetRef) {
  const attach = (input, captured) => {
    if (!input) return;
    const handler = async () => {
      const file = input.files && input.files[0];
      if (!file) return;
      try {
        const s = states.get(input.__owner) || {};
        const base64 = await toJpegBase64(file, s.maxSize || 1600, (s.quality || 80) / 100);
        await dotNetRef.invokeMethodAsync('OnWebFileReady', base64, captured);
      } catch (e) {
        await dotNetRef.invokeMethodAsync('OnWebFileError', String(e && e.message || e));
      } finally {
        input.value = ''; // 同じファイルをもう一度選べるように
      }
    };
    input.__owner = camera;
    input.addEventListener('change', handler);
    input.__handler = handler;
  };
  states.set(camera, { maxSize: 1600, quality: 80 });
  attach(camera, true);
  attach(library, false);
}

export function setOptions(camera, maxSize, quality) {
  states.set(camera, { maxSize, quality });
}

export function open(input) {
  input.click();
}

export function dispose(camera, library) {
  for (const i of [camera, library]) {
    if (i && i.__handler) i.removeEventListener('change', i.__handler);
  }
}

async function toJpegBase64(file, maxSize, quality) {
  const bmp = await createImageBitmap(file, { imageOrientation: 'from-image' });
  const scale = Math.min(1, maxSize / Math.max(bmp.width, bmp.height));
  const w = Math.max(1, Math.round(bmp.width * scale));
  const h = Math.max(1, Math.round(bmp.height * scale));
  const canvas = document.createElement('canvas');
  canvas.width = w;
  canvas.height = h;
  const ctx = canvas.getContext('2d');
  ctx.fillStyle = '#ffffff'; // 透過 PNG 等を JPEG にするときの背景
  ctx.fillRect(0, 0, w, h);
  ctx.drawImage(bmp, 0, 0, w, h);
  if (bmp.close) bmp.close();
  const blob = await new Promise((resolve, reject) =>
    canvas.toBlob(b => b ? resolve(b) : reject(new Error('toBlob failed')), 'image/jpeg', quality));
  const buf = new Uint8Array(await blob.arrayBuffer());
  let bin = '';
  const chunk = 0x8000;
  for (let i = 0; i < buf.length; i += chunk) bin += String.fromCharCode.apply(null, buf.subarray(i, i + chunk));
  return btoa(bin);
}
