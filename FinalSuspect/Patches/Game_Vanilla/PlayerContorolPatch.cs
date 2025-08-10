using AmongUs.GameOptions;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using TMPro;
using UnityEngine;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
internal class MurderPlayerPatch
{
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        target.SetRealKiller(__instance);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CoSetRole))]
internal class CoSetRolePatch
{
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] RoleTypes roleTypes)
    {
        try
        {
            __instance.SetRole(roleTypes);
        }
        catch
        {
            /* ignored */
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
internal class PlayerStartPatch
{
    private const float RoleTextSize = 2f;

    public static void Postfix(PlayerControl __instance)
    {
        var topText = Object.Instantiate(__instance.cosmetics.nameText, __instance.cosmetics.nameText.transform, true);
        topText.text = topText.gameObject.name = "TopText";
        topText.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        topText.transform.localScale = new Vector3(1f, 1f, 1f);
        topText.fontSize = RoleTextSize;
        topText.text = "TopText";
        topText.gameObject.name = "TopText";
        topText.enabled = false;
        topText.alignment = TextAlignmentOptions.Bottom;

        var bottomText =
            Object.Instantiate(__instance.cosmetics.nameText, __instance.cosmetics.nameText.transform, true);
        bottomText.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        bottomText.transform.localScale = new Vector3(1f, 1f, 1f);
        bottomText.fontSize = RoleTextSize;
        bottomText.text = "BottomText";
        bottomText.gameObject.name = "BottomText";
        bottomText.enabled = false;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetTasks))]
internal class PlayerControlSetTasksPatch
{
    public static void Postfix(PlayerControl __instance,
        [HarmonyArgument(0)] Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo> tasks)
    {
        // 自由模式假人处理
        if (__instance.GetFinalData() == null)
            FinalPlayerData.CreateDataFor(__instance);
        __instance.SetTaskTotalCount(tasks.Count);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
internal class PlayerControlCompleteTaskPatch
{
    public static void Postfix(PlayerControl __instance)
    {
        Info($"TaskComplete:{__instance.GetNameWithRole()}", "CompleteTask");
        __instance.OnCompleteTask();

        GameData.Instance.RecomputeTaskCounts();
        Info($"TotalTaskCounts: {GameData.Instance.CompletedTasks}/{GameData.Instance.TotalTasks}",
            "TaskState.Update");
    }
}