namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
public class ConstantsPatch
{
    public static void Postfix(ref int __result)
    {
        if (IsLocalGame) Info($"IsLocalGame: {__result}", "VersionServer");
        if (IsOnlineGame) Info($"IsOnlineGame: {__result}", "VersionServer");
    }
}