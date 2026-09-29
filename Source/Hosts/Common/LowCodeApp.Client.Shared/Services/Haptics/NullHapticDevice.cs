namespace LowCodeApp.Client.Shared.Services
{
    public class NullHapticDevice : IHapticDevice
    {
        public bool IsSupported => false;
        public Task PerformAsync(HapticKind kind) => Task.CompletedTask;
        public Task<bool?> LoadSettingAsync(string key) => Task.FromResult<bool?>(null);
        public Task SaveSettingAsync(string key, bool value) => Task.CompletedTask;
    }
}
