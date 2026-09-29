using LowCodeApp.Client.Shared.Services;

namespace LowCodeApp.Maui.Services
{
    /// <summary>MAUI の Geolocation による現在位置。失敗・権限拒否・タイムアウトは null。</summary>
    public class MauiDeviceLocation : IDeviceLocation
    {
        public Task<DeviceLocationResult?> GetAsync(TimeSpan timeout)
            => MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    var status = await MauiPermissionGate.EnsureAsync<Permissions.LocationWhenInUse>();
                    if (status != PermissionStatus.Granted) return null;

                    Location? loc = null;
                    try { loc = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High, timeout)); }
                    catch { /* 下の最後に取得できた位置にフォールバック */ }
                    loc ??= await Geolocation.Default.GetLastKnownLocationAsync();
                    if (loc == null) return null;
                    return new DeviceLocationResult { Latitude = loc.Latitude, Longitude = loc.Longitude, Accuracy = loc.Accuracy };
                }
                catch { return null; }
            });
    }
}
