using AmongUs.GameOptions;
using HarmonyLib;
using Hazel.Dtls;
using LibCpp2IL.Elf;
using System.Linq;
using System.Text;
using TMPro;



using UnityEngine;
using static FinalSuspect_Xtreme.Translator;

namespace FinalSuspect_Xtreme;

[HarmonyPatch(typeof(Vent), nameof(Vent.SetOutline))]
class SetVentOutlinePatch
{
    public static void Postfix(Vent __instance, [HarmonyArgument(1)] ref bool mainTarget)
    {
        var player = PlayerControl.LocalPlayer;
        Color color = PlayerControl.LocalPlayer.GetRoleColor();
        __instance.myRend.material.SetColor("_OutlineColor", color);
        __instance.myRend.material.SetColor("_AddColor", mainTarget ? color : Color.clear);
    }
}
[HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.SetTaskText))]
class TaskPanelBehaviourPatch
{
    // タスク表示の文章が更新・適用された後に実行される
    public static void Postfix(TaskPanelBehaviour __instance)
    {
        if (!GameStates.IsInGame) return;
        PlayerControl player = PlayerControl.LocalPlayer;
        var role = player.GetRoleType();
        var taskText = __instance.taskText.text;
        if (taskText == "None") return;

        // 役職説明表示

        var RoleWithInfo = $"{Utils.GetRoleName(role)}:\r\n";
        RoleWithInfo += role.GetRoleInfoForVanilla();

        var AllText = Utils.ColorString(player.GetRoleColor(), RoleWithInfo);


        var lines = taskText.Split("\r\n</color>\n")[0].Split("\r\n\n")[0].Split("\r\n");
        StringBuilder sb = new();
        foreach (var eachLine in lines)
        {
            var line = eachLine.Trim();
            if ((line.StartsWith("<color=#FF1919FF>") || line.StartsWith("<color=#FF0000FF>")) && sb.Length < 1 && !line.Contains('(')) continue;
            sb.Append(line + "\r\n");
        }
        if (sb.Length > 1)
        {
            var text = sb.ToString().TrimEnd('\n').TrimEnd('\r');
            if (player.IsImpostor() && sb.ToString().Count(s => (s == '\n')) >= 2)
                text = $"{Utils.ColorString(new Color32(255, 20, 147, byte.MaxValue), GetString("FakeTask"))}\r\n{text}";
            AllText += $"\r\n\r\n<size=85%>{text}</size>";
        }

        AllText += $"\r\n\r\n</color><size=70%>{GetString("PressF1ShowRoleDescription")}</size>";




        __instance.taskText.text = AllText;


        // RepairSenderの表示
        if (RepairSender.enabled && AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame)
            __instance.taskText.text = RepairSender.GetText();
    }
}

class RepairSender
{
    public static bool enabled = false;
    public static bool TypingAmount = false;

    public static int SystemType;
    public static int amount;

    public static void Input(int num)
    {
        if (!TypingAmount)
        {
            //SystemType入力中
            SystemType *= 10;
            SystemType += num;
        }
        else
        {
            //Amount入力中
            amount *= 10;
            amount += num;
        }
    }
    public static void InputEnter()
    {
        if (!TypingAmount)
        {
            //SystemType入力中
            TypingAmount = true;
        }
        else
        {
            //Amount入力中
            Send();
        }
    }
    public static void Send()
    {
        ShipStatus.Instance.RpcUpdateSystem((SystemTypes)SystemType, (byte)amount);
        Reset();
    }
    public static void Reset()
    {
        TypingAmount = false;
        SystemType = 0;
        amount = 0;
    }
    public static string GetText()
    {
        return SystemType.ToString() + "(" + ((SystemTypes)SystemType).ToString() + ")\r\n" + amount;
    }
}