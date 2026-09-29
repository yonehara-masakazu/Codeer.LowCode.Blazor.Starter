// HID キーボードとして接続されたバーコードスキャナ用の入力欄。
// 入力値は Blazor にバインドせず、Enter で取り出して即座に空に戻し、.NET (OnCode) へ渡す。
const states = new WeakMap();

// 利用者が別の入力欄へ移ったときだけフォーカスを譲る。
// ボタンを押した後などはスキャンを続けられるよう入力欄に戻す
function isOtherEditable(el, input) {
    if (!el || el === input) return false;
    if (el.isContentEditable) return true;
    const tag = el.tagName;
    if (tag === 'TEXTAREA' || tag === 'SELECT') return true;
    if (tag !== 'INPUT') return false;
    const type = (el.type || '').toLowerCase();
    return !['button', 'submit', 'reset', 'checkbox', 'radio', 'file', 'image', 'range', 'color'].includes(type);
}

export function init(input, dotnetRef, options) {
    if (!input) return;
    dispose(input);
    const state = { dotnetRef, keepFocus: !!(options && options.keepFocus), timer: null };

    state.onKeyDown = (e) => {
        if (e.key === 'Enter' && !e.isComposing) {
            e.preventDefault();
            e.stopPropagation();
            const v = input.value;
            input.value = '';
            if (v !== '') {
                state.dotnetRef.invokeMethodAsync('OnCode', v).catch(() => { });
            }
        }
    };
    state.onFocusOut = () => {
        if (!state.keepFocus) return;
        clearTimeout(state.timer);
        state.timer = setTimeout(() => {
            if (!state.keepFocus || !input.isConnected || input.disabled) return;
            // ダイアログ等が開いている間は奪わない
            if (document.querySelector('dialog[open], .modal.show')) return;
            if (!isOtherEditable(document.activeElement, input)) input.focus();
        }, 50);
    };
    input.addEventListener('keydown', state.onKeyDown);
    input.addEventListener('focusout', state.onFocusOut);
    states.set(input, state);

    if (state.keepFocus && !input.disabled) input.focus();
}

export function setOptions(input, options) {
    const state = states.get(input);
    if (!state || !options) return;
    state.keepFocus = !!options.keepFocus;
}

export function focus(input) {
    if (input) input.focus();
}

export function dispose(input) {
    const state = input && states.get(input);
    if (!state) return;
    clearTimeout(state.timer);
    input.removeEventListener('keydown', state.onKeyDown);
    input.removeEventListener('focusout', state.onFocusOut);
    states.delete(input);
}
