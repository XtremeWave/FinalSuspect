using FinalSuspect.Attributes;
using FinalSuspect.DataHandling.FinalGameData;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
public class ShipStatusStartPatch
{
    public static void Postfix()
    {
        Info("-----------游戏开始-----------", "Phase");
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
public class AmongUsClientOnGameEndPatch
{
    public static void Postfix()
    {
        UpdateGameState(false, StateTypes.InGame);
        UpdateGameState(false, StateTypes.InitGame);
        Info("-----------游戏结束-----------", "Phase");
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
[HarmonyPriority(Priority.First)]
public class MeetingHudStartPatch
{
    public static void Prefix()
    {
        _ = new LateTask(() => UpdateGameState(true, StateTypes.InMeeting), 1f, "Update game state");
        Info("------------会议开始------------", "Phase");
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
internal class MeetingHudOnDestroyPatch
{
    public static void Postfix()
    {
        UpdateGameState(false, StateTypes.InMeeting);
        Info("------------会议结束------------", "Phase");
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
internal class CoStartGamePatch
{
    public static void Prefix()
    {
        UpdateGameState(true, StateTypes.InitGame);
    }

    public static void Postfix()
    {
        GameModuleInitializerAttribute.InitializeAll();
    }
}

/*[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGameHost))]
internal class CoStartGameHPatch
{
    public static void Prefix()
    {
        foreach (var client in AmongUsClient.Instance.allClients)
        {
            client.IsReady = true;
        }
    }

    public static void Postfix()
    {
        var clientData = GetPlayerById(1).GetFinalData().CheatData.ClientData;

        AmongUsClient.Instance.SendLateRejection(clientData.Id, DisconnectReasons.ClientTimeout);
        clientData.IsReady = true;
        AmongUsClient.Instance.OnPlayerLeft(clientData, DisconnectReasons.ClientTimeout);
    }
}*/

#if Windows
[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
#elif Android
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.StartSFX))]
[HarmonyPatch(typeof(FungleShipStatus), nameof(FungleShipStatus.StartSFX))]
#endif
public static class IntroCutsceneOnDestroyPatch
{
    public static void Postfix()
    {
        FinalGameData.IntroDestroyed = true;
        Info("OnDestroy", "IntroCutscene");
    }
}