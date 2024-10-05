using HarmonyLib;

namespace FinalSuspect;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
class ShipStatusStartPatch
{
    public static void Postfix()
    {
        Logger.Info("-----------游戏开始-----------", "Phase");
    }
}
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
class AmongUsClientOnGameEndPatch
{
    public static void Postfix()
    {
        XtremeGameData.GameStates.InGame = false;
        Logger.Info("-----------游戏结束-----------", "Phase");
    }
}
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
[HarmonyPriority(Priority.First)]
class MeetingHudStartPatch
{
    public static void Prefix()
    {
        Logger.Info("------------会议开始------------", "Phase");
    }
}
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
class MeetingHudOnDestroyPatch
{
    public static void Postfix()
    {
        Logger.Info("------------会议结束------------", "Phase");
    }
}
[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
public static class IntroCutsceneOnDestroyPatch
{
    public static bool introDestroyed;
    public static void Postfix()
    {
        introDestroyed = true;
        Logger.Info("OnDestroy", "IntroCutscene");
    }
}