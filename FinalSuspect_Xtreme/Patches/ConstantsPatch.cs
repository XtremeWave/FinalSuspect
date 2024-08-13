using HarmonyLib;

namespace FinalSuspect_Xtreme.Patches;

[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
class ServerUpdatePatch
{
    static void Postfix(ref int __result)
    {
        if (XtremeGameData.GameStates.IsLocalPlayerGame)
        {
            Logger.Info($"IsLocalPlayerGame: {__result}", "VersionServer");
        }
        if (XtremeGameData.GameStates.IsOnlineGame)
        {
            if (ServerManager.Instance.CurrentRegion.Name is "Niko233[AS_CN]" or "Niko233[NA_US]")
                __result += 25;
            Logger.Info($"IsOnlineGame: {__result}", "VersionServer");
        }
    }
}