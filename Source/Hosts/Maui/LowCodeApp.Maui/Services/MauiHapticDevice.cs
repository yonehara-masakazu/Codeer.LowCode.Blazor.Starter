using LowCodeApp.Client.Shared.Services;

namespace LowCodeApp.Maui.Services
{
    /// <summary>MAUI 端末の振動。プラットフォーム固有部分は Platforms/ 側の partial。設定は Preferences。</summary>
    public partial class MauiHapticDevice : IHapticDevice
    {
        public bool IsSupported => PlatformIsSupported();

        public Task PerformAsync(HapticKind kind)
        {
            try { PlatformPerform(kind); } catch { }
            return Task.CompletedTask;
        }

        //Preferences は軽いので毎回読む (設定画面での変更が即時に効く)
        public Task<bool?> LoadSettingAsync(string key)
        {
            try
            {
                if (!Preferences.Default.ContainsKey(key)) return Task.FromResult<bool?>(null);
                return Task.FromResult<bool?>(Preferences.Default.Get(key, false));
            }
            catch { return Task.FromResult<bool?>(null); }
        }

        public Task SaveSettingAsync(string key, bool value)
        {
            try { Preferences.Default.Set(key, value); } catch { }
            return Task.CompletedTask;
        }

        partial void PlatformPerformImpl(HapticKind kind);
        void PlatformPerform(HapticKind kind) => PlatformPerformImpl(kind);

#if !ANDROID && !IOS
        static bool PlatformIsSupported() => false;
#endif
    }
}
