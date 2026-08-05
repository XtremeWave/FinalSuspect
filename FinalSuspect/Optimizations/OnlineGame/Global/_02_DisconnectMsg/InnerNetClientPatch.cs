using FinalSuspect.Modules.Core.Game.UI;
using InnerNet;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch(typeof(InnerNetClient))]
public class InnerNetClientPatch
{
    [HarmonyPatch(nameof(InnerNetClient.DisconnectInternal))]
    [HarmonyPrefix]
    public static void DisconnectInternal_Prefix(InnerNetClient __instance, DisconnectReasons reason, string stringReason)
    {
        try
        {
            DisconnectPopupPatch.Reason = reason;
            DisconnectPopupPatch.StringReason = stringReason;

            Info($"断开连接(理由:{reason}:{stringReason}，Ping:{__instance.Ping})", "Session");
            FinalPlayerData.DisposeAll();
            LastResult.DestroyAll();

            ErrorText.Instance.cheatDetected = false;
            ErrorText.Instance.SBDetected = false;
            ErrorText.Instance.Clear();
            //Cloud.StopConnect();
        }
        catch
        {
            /* ignored */
        }
    }
}