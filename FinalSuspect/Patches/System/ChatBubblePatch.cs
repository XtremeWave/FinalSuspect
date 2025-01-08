using FinalSuspect.Helpers;
using UnityEngine;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch(typeof(ChatBubble))]
public static class ChatBubblePatch
{
    private static bool IsModdedMsg(string name) => name.EndsWith('\0');


    [HarmonyPatch(nameof(ChatBubble.SetText)), HarmonyPrefix]
    public static void SetText_Prefix(ChatBubble __instance, ref string chatText)
    {
        var bgcolor = ColorHelper.HalfModColor32;
        var sr = __instance.Background;
        Color namecolor= ColorHelper.FaultColor;
        string name = null;
        bool modded = IsModdedMsg(__instance.playerInfo.PlayerName);
        if (__instance?.playerInfo?.PlayerId == null)
        {
            bgcolor = ColorHelper.HalfYellow;
        }
        else if (modded)
        {
            bgcolor = Color.black;
            namecolor = ColorHelper.TeamColor32;
            chatText = StringHelper.ColorString(Color.white, chatText.TrimEnd('\0'));
            __instance.SetLeft();
        }
        else if (__instance.NameText.color == Color.green)
        {
            bgcolor = ColorHelper.HalfYellow;
            namecolor = ColorHelper.TeamColor32;
        }
        else
        {
            XtremeLocalHandling.GetChatBubbleText(
                __instance.playerInfo.PlayerId,
                ref name,
                ref bgcolor, 
                ref namecolor
            );
            
        }
        
        __instance.NameText.color = namecolor;
        __instance.NameText.text = name ?? __instance.NameText.text;
        sr.color = bgcolor;

        
    }
}