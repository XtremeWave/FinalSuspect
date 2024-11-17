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
            if (ServerManager.Instance.CurrentRegion.Name is "Niko233[AS_CN]" or "Niko233[NA_US]")
                __result += 25;
            Logger.Info($"IsOnlineGame: {__result}", "VersionServer");
        }
    }
}