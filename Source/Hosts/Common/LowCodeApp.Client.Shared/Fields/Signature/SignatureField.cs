using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Script;

namespace LowCodeApp.Client.Shared.Fields.Signature
{
    public class SignatureField : FileField
    {
        public SignatureField(SignatureFieldDesign design) : base(design) { }

        // Design は FileField 側の型 (FileFieldDesign) で公開されているため、
        // 独自プロパティ (CanvasHeight / PenColor / PenWidth) を読むにはこちらを使う。
        public new SignatureFieldDesign Design => (SignatureFieldDesign)base.Design;

        // signature.js から PNG (Base64) を受け取り、FileField の保存フローに乗せる。
        [ScriptHide]
        public async Task SetSignatureImageAsync(byte[] pngBytes)
        {
            await SetFileAsync("signature.png", new StreamContent(new MemoryStream(pngBytes)));
        }
    }
}
