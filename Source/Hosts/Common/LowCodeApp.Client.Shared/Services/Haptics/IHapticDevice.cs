namespace LowCodeApp.Client.Shared.Services
{
    /// <summary>端末ごとの振動の実装。Haptics サービスが使う。</summary>
    public interface IHapticDevice
    {
        bool IsSupported { get; }
        Task PerformAsync(HapticKind kind);
        /// <summary>端末に保存した設定を読む。無ければ null。</summary>
        Task<bool?> LoadSettingAsync(string key);
        Task SaveSettingAsync(string key, bool value);
    }
}
