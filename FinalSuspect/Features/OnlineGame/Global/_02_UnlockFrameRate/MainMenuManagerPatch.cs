namespace FinalSuspect.Features.OnlineGame.Global._02_UnlockFrameRate;

[HarmonyPatch(typeof(MainMenuManager))]
public class MainMenuManagerPatch
{
    [HarmonyPatch(nameof(MainMenuManager.Start))]
    [HarmonyPostfix, HarmonyPriority(Priority.LowerThanNormal)]
    public static void Start_Postfix(MainMenuManager __instance)
    {
        FrameRateManager.AdjustFrameRate();
    }
}