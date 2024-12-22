using AmongUs.GameOptions;
using FinalSuspect.Player;
using HarmonyLib;
using UnityEngine;

namespace FinalSuspect.Patches;

[HarmonyPatch(typeof(ChatBubble))]
public static class ChatBubblePatch
{
    private static bool IsModdedMsg(string name) => name.EndsWith('\0');


    [HarmonyPatch(nameof(ChatBubble.SetText)), HarmonyPrefix]
    public static void SetText_Prefix(ChatBubble __instance, ref string chatText)
    {
        var bgcolor = ColorHelper.HalfModColor32;
        var sr = __instance.Background;
        Color namecolor;
        string name;

        if (__instance?.playerInfo?.PlayerId == null)
        {
            bgcolor = ColorHelper.HalfYellow;
            namecolor = ColorHelper.FaultColor;
            name = default;
            goto EndOfChat;
        }
        bool modded = IsModdedMsg(__instance.playerInfo.PlayerName);

        if (modded)
        {
            sr.color = Color.black;
            chatText = Utils.ColorString(Color.white, chatText.TrimEnd('\0'));
            __instance.SetLeft();
            return;
        }
        
        var player = Utils.GetPlayerById(__instance.playerInfo.PlayerId);
        name = player.CheckAndGetNameWithDetails(out namecolor, out string headdertext);
        
        if (!player.IsAlive())
            bgcolor = new Color32(255, 0, 0, 120);
        if (__instance.NameText.color == Color.green)
        {
            bgcolor = ColorHelper.HalfYellow;
            namecolor = ColorHelper.TeamColor32;
        }
        
        EndOfChat:
        __instance.NameText.color = namecolor;
        __instance.NameText.text = name;
        sr.color = bgcolor;

        
    }
}