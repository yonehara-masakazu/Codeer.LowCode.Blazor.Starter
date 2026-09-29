using Microsoft.JSInterop;

namespace LowCodeApp.Client.Shared.Services
{
    /// <summary>navigator.geolocation による現在位置。すべての失敗は null。</summary>
    public class WebDeviceLocation : IDeviceLocation
    {
        readonly IJSRuntime _js;
        IJSObjectReference? _module;

        public WebDeviceLocation(IJSRuntime js) => _js = js;

        public async Task<DeviceLocationResult?> GetAsync(TimeSpan timeout)
        {
            try
            {
                _module ??= await _js.InvokeAsync<IJSObjectReference>("import", "./_content/LowCodeApp.Client.Shared/location.js");
                return await _module.InvokeAsync<DeviceLocationResult?>("getPosition", (int)timeout.TotalMilliseconds);
            }
            catch { return null; }
        }
    }
}
