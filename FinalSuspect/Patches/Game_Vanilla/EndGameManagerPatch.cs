﻿namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch]
public class EndGameManagerPatch
{
    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.ShowButtons)), HarmonyPostfix]
    public static void ShowButtons_Postfix(EndGameManager __instance)
    {
        if (!Main.AutoEndGame.Value) return;
            new LateTask(()=>
        {
                __instance.Navigation.NextGame();
        }, 2f, "Auto End Game");
    }
}
[HarmonyPatch]
public class ControllerNavMenuPatch
{
    [HarmonyPatch(typeof(ControllerNavMenu), nameof(ControllerNavMenu.Start)), HarmonyPostfix]
    public static void Start_Postfix(ControllerNavMenu __instance)
    {
        if (!Main.AutoEndGame.Value) return;
            __instance.gameObject.SetActive(false);  
    }
}
[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
class GameEndChecker
{
    public static bool Prefix()
    {
        return !(Main.NoGameEnd.Value && DebugModeManager.AmDebugger);
    }
}
