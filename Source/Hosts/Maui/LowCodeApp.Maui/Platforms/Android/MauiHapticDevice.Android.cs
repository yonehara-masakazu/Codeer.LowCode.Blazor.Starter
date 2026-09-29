using Android.Content;
using Android.OS;
using LowCodeApp.Client.Shared.Services;

namespace LowCodeApp.Maui.Services
{
    public partial class MauiHapticDevice
    {
        static Vibrator? _vibrator;

        static Vibrator? GetVibrator()
        {
            if (_vibrator != null) return _vibrator;
            var context = Android.App.Application.Context;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                _vibrator = (context.GetSystemService(Context.VibratorManagerService) as VibratorManager)?.DefaultVibrator;
            else
#pragma warning disable CA1422
                _vibrator = context.GetSystemService(Context.VibratorService) as Vibrator;
#pragma warning restore CA1422
            return _vibrator;
        }

        static bool PlatformIsSupported() => GetVibrator()?.HasVibrator == true;

        partial void PlatformPerformImpl(HapticKind kind)
        {
            var v = GetVibrator();
            if (v == null || !v.HasVibrator) return;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                switch (kind)
                {
                    case HapticKind.Success: Predefined(v, VibrationEffect.EffectClick); break;
                    case HapticKind.Warning: Predefined(v, VibrationEffect.EffectDoubleClick); break;
                    //Warning (ダブルクリック) と区別できるよう、Error は最後を長く鳴らす
                    case HapticKind.Error: ErrorBuzz(v); break;
                    case HapticKind.Selection: Predefined(v, VibrationEffect.EffectTick); break;
                    case HapticKind.Impact: Predefined(v, VibrationEffect.EffectHeavyClick); break;
                }
                return;
            }

            //API 26-28 は Web と同じ長さの波形 (ms)
            switch (kind)
            {
                case HapticKind.Success: OneShot(v, 40); break;
                case HapticKind.Warning: Waveform(v, 0, 40, 60, 40); break;
                case HapticKind.Error: ErrorBuzz(v); break;
                case HapticKind.Selection: OneShot(v, 10); break;
                case HapticKind.Impact: OneShot(v, 25); break;
            }
        }

        //強く 2 回刻んでから 200ms 鳴らす。振幅を指定できない端末は長さだけで同じ形にする
        static void ErrorBuzz(Vibrator v)
        {
            long[] timings = [0, 60, 60, 60, 60, 200];
            if (v.HasAmplitudeControl)
                v.Vibrate(VibrationEffect.CreateWaveform(timings, [0, 255, 0, 255, 0, 255], -1)!);
            else
                Waveform(v, timings);
        }

        static void Predefined(Vibrator v, int effect) => v.Vibrate(VibrationEffect.CreatePredefined(effect)!);

        static void OneShot(Vibrator v, long ms)
            => v.Vibrate(VibrationEffect.CreateOneShot(ms, VibrationEffect.DefaultAmplitude)!);

        static void Waveform(Vibrator v, params long[] timings)
            => v.Vibrate(VibrationEffect.CreateWaveform(timings, -1)!);
    }
}
