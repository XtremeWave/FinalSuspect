using System;
using FinalSuspect.Attributes;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using FinalSuspect.Modules.Core.Game.UI;

namespace FinalSuspect.Modules.Features;

// 来源：https://github.com/tugaru1975/TownOfPlus/TOPmods/Zoom.cs
// 参考：https://github.com/Yumenopai/TownOfHost_Y
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class Zoom
{
    private static bool ResetButtons;

    public static void Postfix()
    {
        try
        {
            var canZoom = IsShip || IsLobby || IsFreePlay;

            if (!canZoom || !CanSeeOthersRole() || IsInMeeting || !IsCanMove || InGameRoleInfoMenu.Showing)
            {
                Flag.Run(() => { SetZoomSize(reset: true); }, "Zoom");
                return;
            }

            if (Camera.main?.orthographicSize > 3.0f) ResetButtons = true;
            switch (Input.mouseScrollDelta.y)
            {
                case > 0:
                {
                    if (Camera.main?.orthographicSize > 3.0f)
                        SetZoomSize();
                    break;
                }
                case < 0:
                {
                    if (IsDead || IsFreePlay ||
                        DebugModeManager.IsDebugMode || IsLobby || ConfigManager.GodMode.Value)
                        if (Camera.main?.orthographicSize < 18.0f)
                            SetZoomSize(true);
                    break;
                }
            }

            Flag.NewFlag("Zoom");
        }
        catch
        {
            /* ignored */
        }
    }

    private static void SetZoomSize(bool times = false, bool reset = false)
    {
        if (!Camera.main) return;
        var size = 1.5f;
        if (!times) size = 1 / size;
        if (reset)
        {
            Camera.main.orthographicSize = 3.0f;
            HudManager.Instance.UICamera.orthographicSize = 3.0f;
            HudManager.Instance.Chat.transform.localScale = Vector3.one;
            if (IsInMeeting) MeetingHud.Instance.transform.localScale = Vector3.one;
        }
        else
        {
            Camera.main.orthographicSize *= size;
            HudManager.Instance.UICamera.orthographicSize *= size;
        }

        DestroyableSingleton<HudManager>.Instance?.ShadowQuad?.gameObject.SetActive(
            (reset || Mathf.Approximately(Camera.main.orthographicSize, 3.0f)) && PlayerControl.LocalPlayer.IsAlive());
        if (!ResetButtons) return;
        ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height,
            Screen.fullScreen);
        ResetButtons = false;
    }

    [GameModuleInitializer]
    public static void Init()
    {
        SetZoomSize(reset: true);
    }

    public static void OnFixedUpdate()
    {
        DestroyableSingleton<HudManager>.Instance?.ShadowQuad?.gameObject.SetActive(
            Mathf.Approximately(Camera.main!.orthographicSize, 3.0f) && PlayerControl.LocalPlayer.IsAlive());
    }
}

public static class Flag
{
    private static readonly List<string> OneTimeList = [];
    private static readonly List<string> FirstRunList = [];

    public static void Run(Action action, string type, bool firstrun = false)
    {
        if (!OneTimeList.Contains(type) && (!firstrun || FirstRunList.Contains(type))) return;
        if (!FirstRunList.Contains(type)) FirstRunList.Add(type);
        OneTimeList.Remove(type);
        action();
    }

    public static void NewFlag(string type)
    {
        if (!OneTimeList.Contains(type)) OneTimeList.Add(type);
    }

    public static void DeleteFlag(string type)
    {
        if (OneTimeList.Contains(type)) OneTimeList.Remove(type);
    }
}