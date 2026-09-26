using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.OperatingModel;

namespace LowCodeApp.Client.Shared.Fields.Signature
{
    // 指/ペン/マウスで署名を描いて PNG として保存するフィールド。
    // 保存・更新・一時ファイルの扱いは FileField (組み込み) にそのまま乗る。
    // Canvas への描画とファイル化は SignatureFieldComponent + wwwroot/signature.js が担当する。
    [Designer(DisplayName = "$SignatureField")]
    [ToolboxIcon(PackIconMaterialKind = "Draw")]
    public class SignatureFieldDesign : FileFieldDesign
    {
        public SignatureFieldDesign()
        {
            // JsonAbstract の多態判定に使われるため、派生クラスでは自分の FullName で上書きする。
            TypeFullName = typeof(SignatureFieldDesign).FullName!;
        }

        [Designer(Index = 100, DisplayName = "$SignatureFieldCanvasHeight")]
        public double CanvasHeight { get; set; } = 200;

        [Designer(Index = 101, DisplayName = "$SignatureFieldPenColor")]
        public string PenColor { get; set; } = "#000000";

        [Designer(Index = 102, DisplayName = "$SignatureFieldPenWidth")]
        public double PenWidth { get; set; } = 2.5;

        public override string GetWebComponentTypeFullName() => typeof(SignatureFieldComponent).FullName!;

        public override FieldBase CreateField() => new SignatureField(this);
    }
}
