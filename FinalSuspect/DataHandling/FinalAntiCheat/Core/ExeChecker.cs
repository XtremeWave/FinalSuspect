#if Windows
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
#endif

namespace FinalSuspect.DataHandling.FinalAntiCheat.Core;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
// ReSharper disable once CheckNamespace
internal static class ExeChecker
{
#if Windows
    private static System.Threading.Timer _windowCheckTimer;
    private static bool _isQuitting;
#endif

    public static void Prefix(MainMenuManager __instance)
    {
#if Windows
        // 检测非法进程
        Process[] procs = Process.GetProcessesByName("AmongUsCosmetics");
        if (procs.Length > 0)
        {
            Error($"检测到非法进程: AmongUsCosmetics.exe！游戏将被强制终止。", "FAC");
            Application.Quit(1);
        }

        // 检测非法窗口标题
        IntPtr hWnd = FindWindow(null, "HackerHansen's Among Us Cosmetics Unlocker for v16.1.0s (updated 6/15/2025)");
        if (hWnd != IntPtr.Zero)
        {
            Error($"检测到非法进程: AmongUsCosmetics.exe！游戏将被强制终止。", "FAC");
            Application.Quit(1);
        }

        // 每1分钟检测一次非法窗口标题
        _windowCheckTimer ??= new System.Threading.Timer(_ =>
            {
                if (_isQuitting) return;
                IntPtr hWndTimer = FindWindow(null, "HackerHansen's Among Us Cosmetics Unlocker for v16.1.0s (updated 6/15/2025)");
                if (hWndTimer != IntPtr.Zero)
                {
                    _isQuitting = true;
                    Error($"检测到非法进程: AmongUsCosmetics.exe！游戏将被强制终止。", "FAC");
                    Application.Quit(1);
                }
            }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
#endif
    }

#if Windows
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
#endif
}