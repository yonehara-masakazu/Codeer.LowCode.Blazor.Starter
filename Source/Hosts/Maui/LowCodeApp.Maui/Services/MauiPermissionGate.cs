namespace LowCodeApp.Maui.Services
{
    /// <summary>
    /// 権限の要求を 1 つずつ通す。Android は権限ダイアログを同時に 1 つしか出せないので、
    /// 撮影ボタンでカメラと位置の要求が同時に走ると片方が無視・拒否扱いになることがある。
    /// </summary>
    static class MauiPermissionGate
    {
        static readonly SemaphoreSlim _gate = new(1, 1);

        public static async Task<PermissionStatus> EnsureAsync<T>() where T : Permissions.BasePermission, new()
        {
            await _gate.WaitAsync();
            try
            {
                var status = await Permissions.CheckStatusAsync<T>();
                if (status != PermissionStatus.Granted) status = await Permissions.RequestAsync<T>();
                return status;
            }
            finally { _gate.Release(); }
        }
    }
}
