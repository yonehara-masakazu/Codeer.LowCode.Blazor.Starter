using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Script;

namespace LowCodeApp.Client.Shared.Fields.ScanInput
{
    /// <summary>重複したコードを読み取ったときの動作。</summary>
    public enum ScanDuplicateMode
    {
        [Designer(DisplayName = "$ScanDuplicateMode_Allow")] Allow,
        [Designer(DisplayName = "$ScanDuplicateMode_Warn")] Warn,
        [Designer(DisplayName = "$ScanDuplicateMode_Ignore")] Ignore,
    }

    // キーボード (HID) として接続されたバーコードスキャナで連続読み取りするための入力欄。データは持たない。
    [Designer(DisplayName = "$ScanInputField")]
    [ToolboxIcon(PackIconMaterialKind = "BarcodeScan")]
    [IgnoreBaseProperties(nameof(FieldDesignBase.IgnoreModification), nameof(FieldDesignBase.OnValidateInput))]
    public class ScanInputFieldDesign() : FieldDesignBase(typeof(ScanInputFieldDesign).FullName!)
    {
        [Designer(Index = 3, CandidateType = CandidateType.ScriptEvent, DisplayName = "$ScanInputFieldOnScanned")]
        [ScriptMethod(ArgumentTypes = ["string"], ArgumentNames = ["code"], ReturnType = "bool")]
        public string OnScanned { get; set; } = string.Empty;

        [Designer(Index = 4, DisplayName = "$ScanInputFieldDuplicateMode")]
        public ScanDuplicateMode DuplicateMode { get; set; } = ScanDuplicateMode.Warn;

        [Designer(Index = 5, DisplayName = "$ScanInputFieldUseHaptics")]
        public bool UseHaptics { get; set; } = true;

        [Designer(Index = 6, DisplayName = "$ScanInputFieldKeepFocus")]
        public bool KeepFocus { get; set; } = true;

        [Designer(Index = 7, DisplayName = "$ScanInputFieldShowSoftKeyboard")]
        public bool ShowSoftKeyboard { get; set; } = true;

        [Designer(Index = 8, DisplayName = "$ScanInputFieldPlaceholder")]
        public string Placeholder { get; set; } = string.Empty;

        public override string GetWebComponentTypeFullName() => typeof(ScanInputFieldComponent).FullName!;

        public override string GetSearchWebComponentTypeFullName() => string.Empty;

        public override string GetSearchControlTypeFullName() => string.Empty;

        public override FieldDataBase? CreateData() => null;

        public override FieldBase CreateField() => new ScanInputField(this);
    }
}
