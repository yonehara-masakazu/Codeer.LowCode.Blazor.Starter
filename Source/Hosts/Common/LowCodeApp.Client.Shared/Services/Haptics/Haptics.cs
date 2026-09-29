namespace LowCodeApp.Client.Shared.Services
{
    /// <summary>スクリプトから呼ぶ振動サービス。失敗しても業務処理は止めない。</summary>
    public class Haptics
    {
        const string KeyEnabled = "Haptics.IsEnabled";
        const string KeySelection = "Haptics.IsSelectionEnabled";

        readonly IHapticDevice _device;
        bool? _enabled;
        bool? _selectionEnabled;

        public Haptics(IHapticDevice device)
        {
            _device = device;
            _ = LoadCoreAsync();
        }

        public void Success() => Fire(HapticKind.Success);
        public void Warning() => Fire(HapticKind.Warning);
        public void Error() => Fire(HapticKind.Error);
        public void Selection() => Fire(HapticKind.Selection);
        public void Impact() => Fire(HapticKind.Impact);

        public bool IsSupported => _device.IsSupported;

        /// <summary>全体スイッチ。既定 true。</summary>
        public bool IsEnabled
        {
            get => _enabled ?? true;
            set { _enabled = value; Save(KeyEnabled, value); }
        }

        /// <summary>Selection (刻み) だけの別スイッチ。既定 false。</summary>
        public bool IsSelectionEnabled
        {
            get => _selectionEnabled ?? false;
            set { _selectionEnabled = value; Save(KeySelection, value); }
        }

        void Save(string key, bool value)
        {
            try { _ = _device.SaveSettingAsync(key, value).ContinueWith(_ => { }, TaskScheduler.Default); } catch { }
        }

        async Task LoadCoreAsync()
        {
            try
            {
                //スクリプトが先に setter を呼んでいたらそちらを優先する
                var e = await _device.LoadSettingAsync(KeyEnabled);
                var s = await _device.LoadSettingAsync(KeySelection);
                _enabled ??= e;
                _selectionEnabled ??= s;
            }
            catch { }
        }

        void Fire(HapticKind kind) => _ = FireAsync(kind);

        async Task FireAsync(HapticKind kind)
        {
            try
            {
                //設定は端末側(設定画面など)で変わりうるので、毎回読み直す
                var e = await _device.LoadSettingAsync(KeyEnabled);
                var s = await _device.LoadSettingAsync(KeySelection);
                //読み直した値でキャッシュも更新する (IsEnabled の getter が古い値を返さないように)
                if (e != null) _enabled = e;
                if (s != null) _selectionEnabled = s;
                var enabled = _enabled ?? true;
                var selection = _selectionEnabled ?? false;
                if (!enabled) return;
                if (kind == HapticKind.Selection && !selection) return;
                await _device.PerformAsync(kind);
            }
            catch { }
        }
    }
}
