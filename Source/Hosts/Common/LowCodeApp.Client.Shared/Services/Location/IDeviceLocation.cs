namespace LowCodeApp.Client.Shared.Services
{
    public class DeviceLocationResult
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        /// <summary>水平精度 (m)。不明なら null。</summary>
        public double? Accuracy { get; set; }
    }

    /// <summary>現在位置の取得。失敗・権限拒否・タイムアウトは null (例外は投げない)。</summary>
    public interface IDeviceLocation
    {
        Task<DeviceLocationResult?> GetAsync(TimeSpan timeout);
    }

    public class NullDeviceLocation : IDeviceLocation
    {
        public Task<DeviceLocationResult?> GetAsync(TimeSpan timeout) => Task.FromResult<DeviceLocationResult?>(null);
    }
}
