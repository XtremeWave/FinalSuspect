using AmongUs.GameOptions;
using FinalSuspect.Modules.Core.Game;
using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace FinalSuspect.Patches.Game_Vanilla;


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
class MurderPlayerPatch
{
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        target.SetRealKiller(__instance);
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CoSetRole))]
class CoSetRolePatch
{
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] RoleTypes roleTypes)
    {
        try
        {
            __instance.SetRole(roleTypes);
        }
        catch 
        {
        }
        
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
class PlayerStartPatch
{
    public static void Postfix(PlayerControl __instance)
    {
        var topText = Object.Instantiate(__instance.cosmetics.nameText);
        topText.transform.SetParent(__instance.cosmetics.nameText.transform);
        topText.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        topText.transform.localScale = new(1f, 1f, 1f);
        topText.fontSize = Main.RoleTextSize;
        topText.text = "TopText";
        topText.gameObject.name = "TopText";
        topText.enabled = false;
        var bottomText = Object.Instantiate(__instance.cosmetics.nameText);
        bottomText.transform.SetParent(__instance.cosmetics.nameText.transform);
        bottomText.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        bottomText.transform.localScale = new(1f, 1f, 1f);
        bottomText.fontSize = Main.RoleTextSize;
        bottomText.text = "BottomText";
        bottomText.gameObject.name = "BottomText";
        bottomText.enabled = false;
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetTasks))]
class PlayerControlSetTasksPatch
{
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] List<NetworkedPlayerInfo.TaskInfo> tasks)
    {
        var pc = __instance;
        pc.SetTaskTotalCount(tasks.Count);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
class PlayerControlCompleteTaskPatch
{
    public static void Postfix(PlayerControl __instance)
    {
        var pc = __instance;
        XtremeLogger.Info($"TaskComplete:{pc.GetNameWithRole()}", "CompleteTask");
        pc.OnCompleteTask();

        GameData.Instance.RecomputeTaskCounts();
        XtremeLogger.Info($"TotalTaskCounts = {GameData.Instance.CompletedTasks}/{GameData.Instance.TotalTasks}", "TaskState.Update");
    }
}
