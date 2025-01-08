using FinalSuspect.Attributes;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
class ShipStatusStartPatch
{
    public static void Postfix()
    {
        XtremeLogger.Info("-----------游戏开始-----------", "Phase");
    }
}
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
class AmongUsClientOnGameEndPatch
{
    public static void Postfix()
    {
        XtremeGameData.GameStates.InGame = false;
        XtremeLogger.Info("-----------游戏结束-----------", "Phase");
    }
}
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
[HarmonyPriority(Priority.First)]
class MeetingHudStartPatch
{
    public static void Prefix()
    {
        XtremeLogger.Info("------------会议开始------------", "Phase");
    }
}
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
class MeetingHudOnDestroyPatch
{
    public static void Postfix()
    {
        XtremeLogger.Info("------------会议结束------------", "Phase");
    }
}
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
internal class CoStartGamePatch
{
    public static void Postfix()
    {
        IntroCutsceneOnDestroyPatch.IntroDestroyed = false;
        GameModuleInitializerAttribute.InitializeAll();
    }

}
[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
public static class IntroCutsceneOnDestroyPatch
{
    public static bool IntroDestroyed;
    public static void Postfix()
    {
        IntroDestroyed = true;
        XtremeLogger.Info("OnDestroy", "IntroCutscene");
    }
}