using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.DesignLogic.Check;
using Codeer.LowCode.Blazor.DesignLogic.Refactor;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.OperatingModel;

namespace LowCodeApp.Client.Shared.Fields.Camera
{
    // その場で撮影 (またはライブラリから選択) → 縮小 → 添付するフィールド。
    // 保存・更新・一時ファイルの扱いは FileField (組み込み) にそのまま乗る。
    // 撮影時の位置・日時は、同じモジュールの別フィールドに書き込む。
    [Designer(DisplayName = "$CameraField")]
    [ToolboxIcon(PackIconMaterialKind = "Camera")]
    public class CameraFieldDesign : FileFieldDesign
    {
        public CameraFieldDesign()
        {
            TypeFullName = typeof(CameraFieldDesign).FullName!;
        }

        [Designer(Index = 100, CandidateType = CandidateType.Field, DisplayName = "$CameraFieldLatitudeField")]
        public string LatitudeField { get; set; } = string.Empty;

        [Designer(Index = 101, CandidateType = CandidateType.Field, DisplayName = "$CameraFieldLongitudeField")]
        public string LongitudeField { get; set; } = string.Empty;

        [Designer(Index = 102, CandidateType = CandidateType.Field, DisplayName = "$CameraFieldTakenAtField")]
        public string TakenAtField { get; set; } = string.Empty;

        [Designer(Index = 103, DisplayName = "$CameraFieldMaxImageSize")]
        public int MaxImageSize { get; set; } = 1600;

        [Designer(Index = 104, DisplayName = "$CameraFieldJpegQuality")]
        public int JpegQuality { get; set; } = 80;

        [Designer(Index = 105, DisplayName = "$CameraFieldAllowPickFromLibrary")]
        public bool AllowPickFromLibrary { get; set; } = true;

        public override string GetWebComponentTypeFullName() => typeof(CameraFieldComponent).FullName!;

        public override FieldBase CreateField() => new CameraField(this);

        public override List<DesignCheckInfo> CheckDesign(DesignCheckContext context)
        {
            var result = base.CheckDesign(context);
            context.CheckFieldFieldExistence(Name, nameof(LatitudeField), LatitudeField).AddTo(result);
            context.CheckFieldFieldExistence(Name, nameof(LongitudeField), LongitudeField).AddTo(result);
            context.CheckFieldFieldExistence(Name, nameof(TakenAtField), TakenAtField).AddTo(result);
            return result;
        }

        public override RenameResult ChangeName(RenameContext context) => context.Builder()
            .AddLink(Name, x => Name = x)
            .AddField(NextFocusField, x => NextFocusField = x)
            .AddField(LatitudeField, x => LatitudeField = x)
            .AddField(LongitudeField, x => LongitudeField = x)
            .AddField(TakenAtField, x => TakenAtField = x)
            .Build();
    }
}
