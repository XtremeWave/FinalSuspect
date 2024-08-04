using AmongUs.GameOptions;
using HarmonyLib;
using System.Drawing;
using UnityEngine;

namespace FinalSuspect_Xtreme.Patches;

[HarmonyPatch(typeof(ChatBubble))]
public static class ChatBubblePatch
{

    [HarmonyPatch(nameof(ChatBubble.SetName)), HarmonyPostfix]
    public static void SetName_Postfix(ChatBubble __instance)
    {
        if (GameStates.IsLobby)
        {
            string color;
            if (Main.playerVersion.TryGetValue(__instance.playerInfo.PlayerId, out var ver) && ver != null)
            {
                if (Main.ForkId != ver.forkId)
                {
                    color = "#BFFFB9";
                }
                else if (Main.version.CompareTo(ver.version) == 0)
                {
                    var currectbranch = ver.tag == $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})";
                    color = currectbranch ? "#B1FFE7" : "#ffff00";
                }
                else
                {
                    color = "#ff0000";
                }
            }
            else
            {
                color = __instance.playerInfo.PlayerId == PlayerControl.LocalPlayer.PlayerId ? "#B1FFE7" : "#E1E0B3";
            }
            __instance.NameText.color = ColorHelper.HexToColor(color);
        }
        if (GameStates.IsInGame)
        {
            var dead = CustomPlayerData.GetPlayerDataById(__instance.playerInfo.PlayerId).IsDead;
            if (__instance.playerInfo.PlayerId == PlayerControl.LocalPlayer.PlayerId
                || !PlayerControl.LocalPlayer.IsAlive() && 
                ((dead && PlayerControl.LocalPlayer.GetRoleType() is RoleTypes.GuardianAngel)
                || ( PlayerControl.LocalPlayer.GetRoleType() is not RoleTypes.GuardianAngel)))
            __instance.NameText.color = Utils.GetPlayerById(__instance.playerInfo.PlayerId).GetRoleColor();
        }
        
    }
    [HarmonyPatch(nameof(ChatBubble.SetText)), HarmonyPrefix]
    public static void SetText_Prefix(ChatBubble __instance, ref string chatText)
    {
        var sr = __instance.transform.FindChild("Background").GetComponent<SpriteRenderer>();
        if (Utils.GetPlayerById(__instance.playerInfo.PlayerId).IsAlive())
            sr.color = Main.ModColor32_semi_transparent;
        else
            sr.color = new Color32(255, 0, 0, 120);
    }
}