namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch]
public class EndGameManagerPatch
{
    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.ShowButtons))]
    [HarmonyPostfix]
    public static void ShowButtons_Postfix(EndGameManager __instance)
    {
        if (!Main.AutoEndGame.Value) return;
        DestroyableSingleton<EndGameNavigation>.Instance.ContinueButton.gameObject.SetActive(false);
        _ = new LateTask(__instance.Navigation.NextGame, 2f, "Auto End Game");
    }
}

[HarmonyPatch]
public class GameEndChecker
{
    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
    [HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlowHnS.CheckEndCriteria))]
    [HarmonyPrefix]
    public static bool CheckEndCriteria()
    {
        return !(Main.NoGameEnd.Value && DebugModeManager.IsDebugMode);
    }
}