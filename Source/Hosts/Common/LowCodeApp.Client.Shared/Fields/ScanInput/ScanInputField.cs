using Codeer.LowCode.Blazor.DataIO;
using System.Text;
using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Script;
using LowCodeApp.Client.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LowCodeApp.Client.Shared.Fields.ScanInput
{
    public enum ScanResultKind { None, Accepted, Duplicate, Rejected }

    public class ScanInputField : FieldBase<ScanInputFieldDesign>
    {
        readonly SemaphoreSlim _queue = new(1, 1);
        readonly HashSet<string> _history = new(StringComparer.Ordinal);

        public ScanInputField(ScanInputFieldDesign design) : base(design) { }

        /// <summary>最後に受理したコード。</summary>
        public string LastCode { get; private set; } = string.Empty;

        /// <summary>受理した件数。</summary>
        public int Count { get; private set; }

        [ScriptHide]
        public ScanResultKind ResultKind { get; private set; }

        [ScriptHide]
        public string ResultCode { get; private set; } = string.Empty;

        //結果が変わるたびに増える。コンポーネントが「どの結果を消したか」を覚えるのに使う
        [ScriptHide]
        public int ResultVersion { get; private set; }

        /// <summary>重複判定の履歴・件数・最後のコード・結果表示をクリアする。</summary>
        public void ClearHistory()
        {
            _history.Clear();
            Count = 0;
            LastCode = string.Empty;
            ResultKind = ScanResultKind.None;
            ResultCode = string.Empty;
            ClearError();
            NotifyStateChanged();
        }

        [ScriptHide]
        public override bool IsModified => false;
        [ScriptHide]
        public override FieldDataBase? GetData() => null;
        [ScriptHide]
        public override FieldSubmitData GetSubmitData() => new();
        [ScriptHide]
        public override async Task InitializeDataAsync(FieldDataBase? fieldDataBase) => await Task.CompletedTask;
        [ScriptHide]
        public override async Task SetDataAsync(FieldDataBase? fieldDataBase) => await Task.CompletedTask;

        /// <summary>コンポーネントから呼ばれる。キューで 1 件ずつ順番に処理する。</summary>
        [ScriptHide]
        public async Task EnqueueAsync(string code)
        {
            await _queue.WaitAsync();
            try { await ProcessAsync(code); }
            catch
            {
                //1 件の失敗でキューを止めない。黙って捨てると読み取りが消えたように見えるので拒否として出す
                try { SetResult(ScanResultKind.Rejected, code ?? string.Empty); Vibrate(ScanResultKind.Rejected); } catch { }
            }
            finally { _queue.Release(); }
        }

        async Task ProcessAsync(string raw)
        {
            var code = ToHalfWidth(raw ?? string.Empty).Trim();
            if (code.Length == 0) return;

            var isDuplicate = _history.Contains(code);
            if (isDuplicate && Design.DuplicateMode == ScanDuplicateMode.Ignore)
            {
                SetResult(ScanResultKind.Duplicate, code);
                Vibrate(ScanResultKind.Duplicate);
                return;
            }

            var accepted = true;
            if (!string.IsNullOrEmpty(Design.OnScanned))
            {
                ClearError();
                try
                {
                    var ret = await Module.ExecuteScriptAsync(Design.OnScanned, code);
                    accepted = ret is not bool b || b;
                }
                catch { accepted = false; }
            }

            if (!accepted)
            {
                SetResult(ScanResultKind.Rejected, code);
                Vibrate(ScanResultKind.Rejected);
                return;
            }

            ClearError();
            _history.Add(code);
            Count++;
            LastCode = code;
            var kind = isDuplicate && Design.DuplicateMode == ScanDuplicateMode.Warn
                ? ScanResultKind.Duplicate : ScanResultKind.Accepted;
            SetResult(kind, code);
            Vibrate(kind);
        }

        //IME がオンのまま読むと全角で届く。string.Normalize(FormKC) は Blazor WebAssembly で
        //例外になることがあるので、スキャナの出力範囲 (全角英数記号とスペース) だけ自前で半角に直す
        static string ToHalfWidth(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (var c in s)
            {
                if (c >= '\uFF01' && c <= '\uFF5E') sb.Append((char)(c - 0xFEE0));
                else if (c == '\u3000') sb.Append(' ');
                else sb.Append(c);
            }
            return sb.ToString();
        }

        void SetResult(ScanResultKind kind, string code)
        {
            ResultKind = kind;
            ResultCode = code;
            ResultVersion++;
            NotifyStateChanged();
        }

        void Vibrate(ScanResultKind kind)
        {
            if (!Design.UseHaptics) return;
            try
            {
                var device = Services.Provider.GetService<IHapticDevice>();
                if (device == null) return;
                var haptics = new Haptics(device);
                switch (kind)
                {
                    case ScanResultKind.Accepted: haptics.Success(); break;
                    case ScanResultKind.Duplicate: haptics.Warning(); break;
                    case ScanResultKind.Rejected: haptics.Error(); break;
                }
            }
            catch { /* 振動できない環境では何もしない */ }
        }
    }
}
