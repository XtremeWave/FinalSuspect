using FinalSuspect.Player;
using HarmonyLib;

namespace FinalSuspect.Patches;

[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
public class ConstantsPatch
{
    public static int Version { get; private set; } = 0;
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

        Version = __result;
    }
}