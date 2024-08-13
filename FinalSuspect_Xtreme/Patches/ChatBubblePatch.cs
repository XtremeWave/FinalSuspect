using AmongUs.GameOptions;
using HarmonyLib;
//using System.Drawing;
using UnityEngine;

namespace FinalSuspect_Xtreme.Patches;

[HarmonyPatch(typeof(ChatBubble))]
public static class ChatBubblePatch
{
    private static bool IsModdedMsg(string name) => name.EndsWith('\0');

    [HarmonyPatch(nameof(ChatBubble.SetName)), HarmonyPostfix]
    public static void SetName_Postfix(ChatBubble __instance, [HarmonyArgument(3)] Color colors)
    {
        
        var player = Utils.GetPlayerById(__instance.playerInfo.PlayerId);

        var __ = "";
        player.GetLobbyText(ref __, out string color);
        __instance.NameText.color = ColorHelper.HexToColor(color);

        if (XtremeGameData.GameStates.IsInGame)
        {
            if (colors == Color.green)
            {
                var sr = __instance.transform.FindChild("Background").GetComponent<SpriteRenderer>();
                sr.color = Main.HalfYellow;
                __instance.NameText.color = Main.TeamColor32;
                return;
            }
            if (Utils.CanSeeOthersRole(player, out bool bothImp) || bothImp)
                __instance.NameText.color = Utils.GetPlayerById(__instance.playerInfo.PlayerId).GetRoleColor();
        }
        
    }
    // ¡È∏–¿¥‘¥£∫YAC

    [HarmonyPatch(nameof(ChatBubble.SetText)), HarmonyPrefix]
    public static void SetText_Prefix(ChatBubble __instance, ref string chatText)
    {
        bool modded = IsModdedMsg(__instance.playerInfo.PlayerName);
        var sr = __instance.transform.FindChild("Background").GetComponent<SpriteRenderer>();
        if (modded)
        {
            sr.color = Color.black;
            chatText = Utils.ColorString(Color.white, chatText.TrimEnd('\0'));
            __instance.SetLeft();
            return;
        }
        if (Utils.GetPlayerById(__instance.playerInfo.PlayerId).IsAlive())
            sr.color = sr.color == Main.HalfYellow ? Main.HalfYellow :Main.ModColor32_semi_transparent;
        else
            sr.color = sr.color == Main.HalfYellow ? Main.HalfYellow : new Color32(255, 0, 0, 120);
    }
}