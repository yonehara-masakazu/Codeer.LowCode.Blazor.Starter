namespace LowCodeApp.Client.Shared.Services
{
    /// <summary>縮小指定。長辺が MaxSize (px) を超える場合に縮小する。</summary>
    public class PhotoOptions
    {
        public int MaxSize { get; set; } = 1600;
        /// <summary>JPEG 品質 (1-100)。</summary>
        public int JpegQuality { get; set; } = 80;
    }

    public class CapturedPhoto
    {
        public byte[] Bytes { get; set; } = Array.Empty<byte>();
        /// <summary>通常は image/jpeg。端末が別形式 (PNG/HEIC 等) で返したときはその形式。</summary>
        public string ContentType { get; set; } = "image/jpeg";
        /// <summary>ドット付き拡張子。</summary>
        public string Extension { get; set; } = ".jpg";
    }

    /// <summary>
    /// 端末のカメラ / 写真ライブラリ。MAUI だけが実装を持つ (Web は CameraFieldComponent が input[type=file] で処理する)。
    /// キャンセルは null。権限拒否などは例外で通知する (呼び出し側がメッセージを表示する)。
    /// </summary>
    public interface ICameraService
    {
        bool IsSupported { get; }
        Task<CapturedPhoto?> CaptureAsync(PhotoOptions options);
        Task<CapturedPhoto?> PickAsync(PhotoOptions options);
    }

    public class NullCameraService : ICameraService
    {
        public bool IsSupported => false;
        public Task<CapturedPhoto?> CaptureAsync(PhotoOptions options) => Task.FromResult<CapturedPhoto?>(null);
        public Task<CapturedPhoto?> PickAsync(PhotoOptions options) => Task.FromResult<CapturedPhoto?>(null);
    }
}
