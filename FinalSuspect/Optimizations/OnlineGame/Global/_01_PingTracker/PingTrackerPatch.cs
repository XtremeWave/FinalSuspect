using System.Text;
using FinalSuspect.Modules.Core.Game.UI;
using FinalSuspect.Modules.Resources;
using FinalSuspect.Patches.System;
using TMPro;

namespace FinalSuspect.Optimizations.OnlineGame.Joined._01_PingTracker;


//[HarmonyPatch(typeof(PingTracker))]
internal static class PingTrackerPatch
{
    private static float _deltaTime;
    private static string _pingLabel;
    private static string _fpsLabel;
    private static string _localLabel;
    private static bool _started;

    private static void Start()
    {
        _pingLabel = GetString("Ping");
        _fpsLabel = GetString("FrameRate");
        _localLabel = GetString("Local");
    }

    //[HarmonyPatch(nameof(PingTracker.Update))]
    //[HarmonyPostfix]
    public static void Update_Postfix(PingTracker __instance)
    {
        if (!_started)
        {
            Start();
            _started = true;
        }

        if (__instance.text.alignment != TextAlignmentOptions.TopGeoAligned)
            __instance.text.alignment = TextAlignmentOptions.TopGeoAligned;

        var ping = AmongUsClient.Instance.Ping;
        var pingColor = ping switch
        {
            < 50 => "#44dfcc",
            < 100 => "#7bc690",
            < 200 => "#f3920e",
            < 400 => "#ff146e",
            _ => "#ff4500"
        };

        _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        var fps = Mathf.Ceil(1.0f / _deltaTime);
        var targetFPS = Application.targetFrameRate;
        if (targetFPS <= 0) targetFPS = 60;

        var ratio = fps / targetFPS;
        var fpsColor = ratio switch
        {
            >= 0.9f => "#4fc3ff",
            >= 0.7f => "#42a5f5",
            >= 0.5f => "#3d5afe",
            _       => "#7c4dff"
        };

        var pingBlock = $"<color={pingColor}>{_pingLabel}:{ping} <size=60%>ms</size></color>";
        var fpsBlock = $"<color={fpsColor}>{_fpsLabel}:{fps} <size=60%>FPS</size></color>";
        var serverBlock = $"<color=#FFD740>◈</color>{(IsOnlineGame ? PingTrackerUpdatePatch.ServerName : _localLabel)}";

        __instance.text.text = 
            $"<pos=0em>{pingBlock}</pos>" +
            $"<pos=5em>{fpsBlock}</pos>" +
            $"<pos=11em>{serverBlock}</pos>";
    }
}