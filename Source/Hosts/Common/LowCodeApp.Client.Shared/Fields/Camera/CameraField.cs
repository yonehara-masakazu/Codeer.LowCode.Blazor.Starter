using System.Net.Http.Headers;
using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Script;
using LowCodeApp.Client.Shared.Services;

namespace LowCodeApp.Client.Shared.Fields.Camera
{
    public class CameraField : FileField
    {
        public CameraField(CameraFieldDesign design) : base(design) { }

        // 独自プロパティ (MaxImageSize など) を読むための型付き Design。
        public new CameraFieldDesign Design => (CameraFieldDesign)base.Design;

        // 撮影 (またはライブラリ選択) した画像を FileField の保存フローに乗せる。
        // captured == true (カメラで撮った) のときだけ、撮影日時・位置を別フィールドに書く。
        // ライブラリから選んだ画像には書かない (既存値にも触らない。証跡として撮影時の情報だけを残すため)。
        [ScriptHide]
        public async Task SetPhotoAsync(CapturedPhoto photo, bool captured, DeviceLocationResult? location, DateTime takenAt)
        {
            //Content-Type を付けないと MAUI (Android) の HttpClient がフォームとして送り、
            //サーバーが本文をフォームとして解析して 500 になる
            var content = new StreamContent(new MemoryStream(photo.Bytes));
            content.Headers.ContentType = new MediaTypeHeaderValue(photo.ContentType);
            await SetFileAsync($"photo_{takenAt:yyyyMMdd_HHmmss}{photo.Extension}", content);

            if (!captured) return;
            if (!string.IsNullOrEmpty(Design.TakenAtField))
            {
                var f = Module.GetField<DateTimeField>(Design.TakenAtField);
                if (f != null) await f.SetValueAsync(takenAt);
            }
            if (location != null)
            {
                if (!string.IsNullOrEmpty(Design.LatitudeField))
                {
                    var f = Module.GetField<NumberField>(Design.LatitudeField);
                    if (f != null) await f.SetValueAsync((decimal)location.Latitude);
                }
                if (!string.IsNullOrEmpty(Design.LongitudeField))
                {
                    var f = Module.GetField<NumberField>(Design.LongitudeField);
                    if (f != null) await f.SetValueAsync((decimal)location.Longitude);
                }
            }
        }
    }
}
