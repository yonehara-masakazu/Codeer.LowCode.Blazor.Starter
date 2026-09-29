// SignatureFieldComponent 用の Canvas 署名パッド。
// Web (WASM/Server) と MAUI BlazorWebView の両方で、JS Interop の範囲だけで完結させる (MAUI 固有 API は使わない)。

const stateMap = new WeakMap();
const COMMIT_DELAY_MS = 800;

function getPoint(canvas, e) {
    const rect = canvas.getBoundingClientRect();
    return { x: e.clientX - rect.left, y: e.clientY - rect.top };
}

function applyDrawingStyle(canvas) {
    const s = stateMap.get(canvas);
    if (!s) return;
    const ctx = canvas.getContext('2d');
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';
    ctx.strokeStyle = s.penColor;
    ctx.lineWidth = s.penWidth;
}

function paintBackground(canvas, w, h) {
    const ctx = canvas.getContext('2d');
    ctx.fillStyle = '#ffffff';
    ctx.fillRect(0, 0, w, h);
}

// canvas の CSS サイズが変わったとき、devicePixelRatio 込みでバッキングストアを張り直す。
// canvas.width/height の再設定はビットマップと描画コンテキストの状態を破棄するため、
// 直前の内容を dataURL として退避し、リサイズ後に描き戻す。
function resize(canvas) {
    const s = stateMap.get(canvas);
    if (!s) return;
    const rect = canvas.getBoundingClientRect();
    const dpr = window.devicePixelRatio || 1;
    const cssW = Math.max(1, Math.round(rect.width));
    const cssH = Math.max(1, Math.round(rect.height));
    if (s.cssW === cssW && s.cssH === cssH) return;

    let prevDataUrl = null;
    if (s.cssW > 0 && s.cssH > 0 && (s.hasInk || s.pendingImage)) {
        try { prevDataUrl = canvas.toDataURL('image/png'); } catch { prevDataUrl = null; }
    }

    s.cssW = cssW;
    s.cssH = cssH;
    canvas.width = Math.max(1, Math.round(cssW * dpr));
    canvas.height = Math.max(1, Math.round(cssH * dpr));

    const ctx = canvas.getContext('2d');
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    paintBackground(canvas, cssW, cssH);
    applyDrawingStyle(canvas);

    const toRestore = prevDataUrl ?? s.pendingImage;
    if (toRestore) {
        const img = new Image();
        img.onload = () => {
            const ctx2 = canvas.getContext('2d');
            ctx2.drawImage(img, 0, 0, cssW, cssH);
        };
        img.src = toRestore;
    }
}

function clearCommitTimer(s) {
    if (s.timer) {
        clearTimeout(s.timer);
        s.timer = null;
    }
}

function scheduleCommit(canvas) {
    const s = stateMap.get(canvas);
    if (!s) return;
    clearCommitTimer(s);
    s.timer = setTimeout(() => {
        s.timer = null;
        if (!s.hasInk) return;
        let dataUrl;
        try { dataUrl = canvas.toDataURL('image/png'); } catch { return; }
        const base64 = dataUrl.substring(dataUrl.indexOf(',') + 1);
        s.dotNetRef.invokeMethodAsync('OnStrokeCompleted', base64).catch(() => { });
    }, COMMIT_DELAY_MS);
}

function onPointerDown(canvas, e) {
    const s = stateMap.get(canvas);
    if (!s) return;
    e.preventDefault();
    // 2 本目以降の指 (や手のひら) は無視する。追わないと直前の点との間を直線で結んでしまう
    if (s.drawing) return;
    clearCommitTimer(s);
    s.drawing = true;
    s.pointerId = e.pointerId;
    try { canvas.setPointerCapture(e.pointerId); } catch { /* ignore */ }
    const p = getPoint(canvas, e);
    s.lastX = p.x;
    s.lastY = p.y;
    // 1点だけのタップでも点が残るように、開始点へ極小の線を描く
    const ctx = canvas.getContext('2d');
    ctx.beginPath();
    ctx.moveTo(p.x, p.y);
    ctx.lineTo(p.x + 0.01, p.y + 0.01);
    ctx.stroke();
    s.hasInk = true;
}

function onPointerMove(canvas, e) {
    const s = stateMap.get(canvas);
    if (!s || !s.drawing) return;
    e.preventDefault();
    if (e.pointerId !== s.pointerId) return;
    const p = getPoint(canvas, e);
    const ctx = canvas.getContext('2d');
    ctx.beginPath();
    ctx.moveTo(s.lastX, s.lastY);
    ctx.lineTo(p.x, p.y);
    ctx.stroke();
    s.lastX = p.x;
    s.lastY = p.y;
}

function onPointerUp(canvas, e) {
    const s = stateMap.get(canvas);
    if (!s) return;
    if (!s.drawing) return;
    e.preventDefault();
    if (e.pointerId !== s.pointerId) return;
    s.drawing = false;
    s.pointerId = null;
    try { canvas.releasePointerCapture(e.pointerId); } catch { /* ignore */ }
    scheduleCommit(canvas);
}

export function init(canvas, dotNetRef, options) {
    if (!canvas) return;
    const s = {
        dotNetRef,
        penColor: (options && options.penColor) || '#000000',
        penWidth: (options && options.penWidth) || 2.5,
        drawing: false,
        hasInk: false,
        pendingImage: null,
        lastX: 0,
        lastY: 0,
        timer: null,
        cssW: 0,
        cssH: 0,
    };
    stateMap.set(canvas, s);

    canvas.style.touchAction = 'none';
    canvas.style.userSelect = 'none';

    s.onPointerDown = (e) => onPointerDown(canvas, e);
    s.onPointerMove = (e) => onPointerMove(canvas, e);
    s.onPointerUp = (e) => onPointerUp(canvas, e);
    canvas.addEventListener('pointerdown', s.onPointerDown);
    canvas.addEventListener('pointermove', s.onPointerMove);
    canvas.addEventListener('pointerup', s.onPointerUp);
    canvas.addEventListener('pointercancel', s.onPointerUp);

    resize(canvas);

    if (typeof ResizeObserver !== 'undefined') {
        s.ro = new ResizeObserver(() => resize(canvas));
        s.ro.observe(canvas);
    }
}

// 保存済みの画像 (data URL) を canvas に描き戻す。以後の描画はこの上に追記される。
export function setImage(canvas, dataUrl) {
    const s = stateMap.get(canvas);
    if (!s) return;
    s.pendingImage = dataUrl || null;
    s.hasInk = !!dataUrl;
    if (!dataUrl) {
        paintBackground(canvas, s.cssW, s.cssH);
        return;
    }
    const img = new Image();
    img.onload = () => {
        const ctx = canvas.getContext('2d');
        paintBackground(canvas, s.cssW, s.cssH);
        ctx.drawImage(img, 0, 0, s.cssW, s.cssH);
    };
    img.src = dataUrl;
}

export function clear(canvas) {
    const s = stateMap.get(canvas);
    if (!s) return;
    clearCommitTimer(s);
    s.hasInk = false;
    s.pendingImage = null;
    paintBackground(canvas, s.cssW, s.cssH);
}

export function setOptions(canvas, options) {
    const s = stateMap.get(canvas);
    if (!s) return;
    if (options && options.penColor) s.penColor = options.penColor;
    if (options && options.penWidth) s.penWidth = options.penWidth;
    applyDrawingStyle(canvas);
}

export function dispose(canvas) {
    const s = stateMap.get(canvas);
    if (!s) return;
    clearCommitTimer(s);
    if (s.ro) s.ro.disconnect();
    canvas.removeEventListener('pointerdown', s.onPointerDown);
    canvas.removeEventListener('pointermove', s.onPointerMove);
    canvas.removeEventListener('pointerup', s.onPointerUp);
    canvas.removeEventListener('pointercancel', s.onPointerUp);
    stateMap.delete(canvas);
}
