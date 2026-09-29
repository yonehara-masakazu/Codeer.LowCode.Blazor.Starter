using Microsoft.JSInterop;

namespace LowCodeApp.Client.Shared.Services
{
    /// <summary>navigator.vibrate による振動。設定は localStorage。すべての例外は握りつぶす。</summary>
    public class WebHapticDevice : IHapticDevice
    {
        readonly IJSRuntime _js;
        IJSObjectReference? _module;
        bool _supported;
        bool _supportChecked;

        public WebHapticDevice(IJSRuntime js) => _js = js;

        //IsSupported は同期プロパティなので、初回の非同期取得が済むまでは false。
        public bool IsSupported
        {
            get
            {
                if (!_supportChecked) _ = EnsureSupportAsync();
                return _supported;
            }
        }

        async Task<IJSObjectReference> GetModuleAsync()
            => _module ??= await _js.InvokeAsync<IJSObjectReference>("import", "./_content/LowCodeApp.Client.Shared/haptics.js");

        async Task EnsureSupportAsync()
        {
            if (_supportChecked) return;
            _supportChecked = true;
            try
            {
                var m = await GetModuleAsync();
                _supported = await m.InvokeAsync<bool>("isSupported");
            }
            catch { }
        }

        public async Task PerformAsync(HapticKind kind)
        {
            try
            {
                var m = await GetModuleAsync();
                await m.InvokeVoidAsync("vibrate", kind.ToString());
            }
            catch { }
        }

        public async Task<bool?> LoadSettingAsync(string key)
        {
            try
            {
                //Haptics の生成時に設定を読みに来るので、ついでに対応可否も取っておく
                //(スクリプトが最初に IsSupported を見たときに false にならないように)
                await EnsureSupportAsync();
                var m = await GetModuleAsync();
                var v = await m.InvokeAsync<string?>("loadSetting", key);
                return v == null ? null : v == "1";
            }
            catch { return null; }
        }

        public async Task SaveSettingAsync(string key, bool value)
        {
            try
            {
                var m = await GetModuleAsync();
                await m.InvokeVoidAsync("saveSetting", key, value ? "1" : "0");
            }
            catch { }
        }
    }
}
