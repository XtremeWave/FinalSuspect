using HarmonyLib;

namespace FinalSuspect;

[HarmonyPatch]
public class EndGameManagerPatch
{
    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.ShowButtons)), HarmonyPostfix]
    public static void ShowButtons_Postfix(EndGameManager __instance)
    {
        if (!Main.AutoEndGame.Value) return;
        new LateTask(__instance.Navigation.NextGame, 2f, "Auto End Game");
    }
}
