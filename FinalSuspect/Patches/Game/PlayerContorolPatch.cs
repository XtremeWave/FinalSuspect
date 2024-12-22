using HarmonyLib;
using System.Text;
using UnityEngine;
using static FinalSuspect.Translator;
using AmongUs.GameOptions;
using static UnityEngine.ParticleSystem.PlaybackState;
using FinalSuspect.Modules.Managers;
using FinalSuspect.Player;
using InnerNet;

namespace FinalSuspect;


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
        __instance.SetRole(roleTypes);
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
class FixedUpdatePatch
{
    public static void Postfix(PlayerControl __instance)
    {
        __instance.OnFixedUpdate();
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
class PlayerStartPatch
{
    public static void Postfix(PlayerControl __instance)
    {
        var roleText = UnityEngine.Object.Instantiate(__instance.cosmetics.nameText);
        roleText.transform.SetParent(__instance.cosmetics.nameText.transform);
        roleText.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        roleText.transform.localScale = new(1f, 1f, 1f);
        roleText.fontSize = Main.RoleTextSize;
        roleText.text = "RoleText";
        roleText.gameObject.name = "RoleText";
        roleText.enabled = false;
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetTasks))]
class PlayerControlSetTasksPatch
{
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo> tasks)
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
        Logger.Info($"TaskComplete:{pc.GetNameWithRole()}", "CompleteTask");
        pc.OnCompleteTask();

        GameData.Instance.RecomputeTaskCounts();
        Logger.Info($"TotalTaskCounts = {GameData.Instance.CompletedTasks}/{GameData.Instance.TotalTasks}", "TaskState.Update");
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckName))]
class CmdCheckNamePatch
{
    public static void Postfix(PlayerControl __instance, ref string name)
    {
        SpamManager.CheckSpam(ref name);
        if (__instance.OwnerId == AmongUsClient.Instance.HostId)
            Main.HostNickName = name;
    }
}
