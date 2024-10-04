using HarmonyLib;
using UnityEngine;

namespace FinalSuspect_Xtreme.Patches;

[HarmonyPatch(typeof(ChatBubble))]
public static class ChatBubblePatch
{
    private static bool IsModdedMsg(string name) => name.EndsWith('\0');


    [HarmonyPatch(nameof(ChatBubble.SetText)), HarmonyPrefix]
    public static void SetText_Prefix(ChatBubble __instance, ref string chatText)
    {
        bool modded = IsModdedMsg(__instance.playerInfo.PlayerName);
        var sr = __instance.Background;

        if (modded)
        {
            sr.color = Color.black;
            chatText = Utils.ColorString(Color.white, chatText.TrimEnd('\0'));
            __instance.SetLeft();
            return;
        }

        var bgcolor = ColorHelper.HalfModColor32;
        //var t = chatText;

        //BoxCollider2D instance_collider = sr.gameObject.AddComponent<BoxCollider2D>();
        //instance_collider.offset = Vector2.zero;
        //instance_collider.size = sr.size;

        //var Button = sr.gameObject.AddComponent<PassiveButton>();
        //var alpha = sr.color.a;

        //Button.Colliders = new Collider2D[] { instance_collider };
        //Button.OnClick = new();
        //Button.OnMouseOut = new();
        //Button.OnMouseOver = new();

        //Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => 
        //{
        //    ClipboardHelper.PutClipboardString(t);
        //    Button.ReceiveMouseOut();
        //}));
        //Button.OnMouseOut.AddListener((UnityEngine.Events.UnityAction)(() => { sr.color.SetAlpha(alpha); }));
        //Button.OnMouseOver.AddListener((UnityEngine.Events.UnityAction)(() => { sr.color.SetAlpha(1f); }));



        var player = Utils.GetPlayerById(__instance.playerInfo.PlayerId);

        var __ = "";
        Color namecolor;
        player.GetLobbyText(ref __, out string color);
        namecolor = ColorHelper.HexToColor(color);


        if (XtremeGameData.GameStates.IsInGame)
        {

            if (Utils.CanSeeOthersRole(player, out bool bothImp) || bothImp)
                namecolor = Utils.GetPlayerById(__instance.playerInfo.PlayerId).GetRoleColor();
            if (!Utils.GetPlayerById(__instance.playerInfo.PlayerId).IsAlive())
                bgcolor = new Color32(255, 0, 0, 120);
            if (__instance.NameText.color == Color.green)
            {
                bgcolor = ColorHelper.HalfYellow;
                namecolor = ColorHelper.TeamColor32;
            }

        }


        __instance.NameText.color = namecolor;
        sr.color = bgcolor;
    }
}