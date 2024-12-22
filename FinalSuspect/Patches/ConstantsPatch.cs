using FinalSuspect.Player;
using HarmonyLib;

namespace FinalSuspect.Patches;

[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
class ServerUpdatePatch
{
    static void Postfix(ref int __result)
    {
        if (XtremeGameData.GameStates.IsLocalGame)
        {
            Logger.Info($"IsLocalGame: {__result}", "VersionServer");
        }
        if (XtremeGameData.GameStates.IsOnlineGame)
        {
            Logger.Info($"IsOnlineGame: {__result}", "VersionServer");
        }
    }
}