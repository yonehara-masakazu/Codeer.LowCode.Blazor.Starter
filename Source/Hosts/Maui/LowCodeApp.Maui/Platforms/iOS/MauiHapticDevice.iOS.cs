using LowCodeApp.Client.Shared.Services;
using UIKit;

namespace LowCodeApp.Maui.Services
{
    public partial class MauiHapticDevice
    {
        //端末に触覚が無ければ OS が黙って無視する
        static bool PlatformIsSupported() => true;

        partial void PlatformPerformImpl(HapticKind kind)
        {
            //UIKit は UI スレッドから呼ぶ
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    switch (kind)
                    {
                        case HapticKind.Success: Notify(UINotificationFeedbackType.Success); break;
                        case HapticKind.Warning: Notify(UINotificationFeedbackType.Warning); break;
                        case HapticKind.Error: Notify(UINotificationFeedbackType.Error); break;
                        case HapticKind.Selection:
                            using (var g = new UISelectionFeedbackGenerator()) { g.Prepare(); g.SelectionChanged(); }
                            break;
                        case HapticKind.Impact:
                            using (var g = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Medium)) { g.Prepare(); g.ImpactOccurred(); }
                            break;
                    }
                }
                catch { }
            });
        }

        static void Notify(UINotificationFeedbackType type)
        {
            using var g = new UINotificationFeedbackGenerator();
            g.Prepare();
            g.NotificationOccurred(type);
        }
    }
}
