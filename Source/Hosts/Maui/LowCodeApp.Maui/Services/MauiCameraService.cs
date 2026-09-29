using LowCodeApp.Client.Shared.Services;

namespace LowCodeApp.Maui.Services
{
    /// <summary>MAUI の MediaPicker による撮影・ライブラリ選択。縮小・回転・品質は MediaPickerOptions に任せる。</summary>
    public class MauiCameraService : ICameraService
    {
        public bool IsSupported => true;

        public Task<CapturedPhoto?> CaptureAsync(PhotoOptions options) => RunAsync(options, capture: true);

        public Task<CapturedPhoto?> PickAsync(PhotoOptions options) => RunAsync(options, capture: false);

        //MediaPicker は UI スレッドから呼ぶ。権限拒否などの例外はそのまま呼び出し側 (コンポーネント) に伝える
        static Task<CapturedPhoto?> RunAsync(PhotoOptions options, bool capture)
            => MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (capture)
                {
                    if (!MediaPicker.Default.IsCaptureSupported) throw new NotSupportedException("Camera is not available on this device.");
                    var status = await MauiPermissionGate.EnsureAsync<Permissions.Camera>();
                    if (status != PermissionStatus.Granted) throw new PermissionException("Camera permission was denied.");
                }

                var o = new MediaPickerOptions
                {
                    MaximumWidth = options.MaxSize,
                    MaximumHeight = options.MaxSize,
                    CompressionQuality = Math.Clamp(options.JpegQuality, 1, 100),
                    RotateImage = true,
                    SelectionLimit = 1,
                };
                var file = capture
                    ? await MediaPicker.Default.CapturePhotoAsync(o)
                    : (await MediaPicker.Default.PickPhotosAsync(o))?.FirstOrDefault();
                if (file == null) return null;

                using var stream = await file.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);

                //JPEG 以外 (PNG / HEIC 等) はそのままのバイトと実際の形式を返す
                var ext = Path.GetExtension(file.FileName);
                if (string.IsNullOrEmpty(ext)) ext = ".jpg";
                var contentType = string.IsNullOrEmpty(file.ContentType) ? GuessContentType(ext) : file.ContentType;
                return new CapturedPhoto { Bytes = ms.ToArray(), ContentType = contentType, Extension = ext.ToLowerInvariant() };
            });

        static string GuessContentType(string ext) => ext.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".heic" => "image/heic",
            ".heif" => "image/heif",
            ".webp" => "image/webp",
            _ => "image/jpeg",
        };
    }
}
