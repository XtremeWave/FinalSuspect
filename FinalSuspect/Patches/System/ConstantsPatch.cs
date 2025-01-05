using FinalSuspect.DataHandling;
using HarmonyLib;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
public class ConstantsPatch
{
    static void Postfix(ref int __result)
    {
        if (XtremeGameData.GameStates.IsLocalGame)
        {
            Modules.Core.Plugin.Logger.Info($"IsLocalGame: {__result}", "VersionServer");
        }
        if (XtremeGameData.GameStates.IsOnlineGame)
        {
            Modules.Core.Plugin.Logger.Info($"IsOnlineGame: {__result}", "VersionServer");
        }
    }
}