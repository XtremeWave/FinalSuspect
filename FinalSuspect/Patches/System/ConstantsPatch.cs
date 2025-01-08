namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
public class ConstantsPatch
{
    static void Postfix(ref int __result)
    {
        if (XtremeGameData.GameStates.IsLocalGame)
        {
            XtremeLogger.Info($"IsLocalGame: {__result}", "VersionServer");
        }
        if (XtremeGameData.GameStates.IsOnlineGame)
        {
            XtremeLogger.Info($"IsOnlineGame: {__result}", "VersionServer");
        }
    }
}