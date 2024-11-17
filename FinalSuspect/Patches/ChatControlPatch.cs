using AmongUs.Data;
using HarmonyLib;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using FinalSuspect.Modules.Managers;
using Il2CppMono.Net;

namespace FinalSuspect;

[HarmonyPatch(typeof(ChatController))]


[HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
public static class ChatControllerUpdatePatch
{
    public static int CurrentHistorySelection = -1;
    public static void Prefix()
    {

        if (AmongUsClient.Instance.AmHost && DataManager.Settings.Multiplayer.ChatMode == InnerNet.QuickChatModes.QuickChatOnly)
            DataManager.Settings.Multiplayer.ChatMode = InnerNet.QuickChatModes.FreeChatOrQuickChat; //コマンドを打つためにホストのみ常時フリーチャット開放

    }
    public static void Postfix(ChatController __instance)
    {
        if (!__instance.freeChatField.textArea.hasFocus) return;
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.C))
            ClipboardHelper.PutClipboardString(__instance.freeChatField.textArea.text);
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.V))
            __instance.freeChatField.textArea.SetText(__instance.freeChatField.textArea.text + GUIUtility.systemCopyBuffer);
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.X))
        {
            ClipboardHelper.PutClipboardString(__instance.freeChatField.textArea.text);
            __instance.freeChatField.textArea.SetText("");
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) && ChatCommands.SentHistory.Count > 0)
        {
            CurrentHistorySelection = Mathf.Clamp(--CurrentHistorySelection, 0, ChatCommands.SentHistory.Count - 1);
            __instance.freeChatField.textArea.SetText(ChatCommands.SentHistory[CurrentHistorySelection]);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) && ChatCommands.SentHistory.Count > 0)
        {
            CurrentHistorySelection++;
            if (CurrentHistorySelection < ChatCommands.SentHistory.Count)
                __instance.freeChatField.textArea.SetText(ChatCommands.SentHistory[CurrentHistorySelection]);
            else __instance.freeChatField.textArea.SetText("");
        }
    }
}
[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
internal class ChatCommands
{
    public static List<string> SentHistory = new();
    public static bool Prefix(ChatController __instance)
    {
        if (__instance.quickChatField.Visible)
        {
            return true;
        }
        if (string.IsNullOrWhiteSpace(__instance.freeChatField.textArea.text))
        {
            return false;
        }

        var text = __instance.freeChatField.textArea.text;
        if (SentHistory.Count == 0 || SentHistory[^1] != text) SentHistory.Add(text);
        ChatControllerUpdatePatch.CurrentHistorySelection = SentHistory.Count;
        return true;
    }
}
[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
internal class ChatAdd
{
    public static void Prefix(ChatController __instance, [HarmonyArgument(0)] PlayerControl sourcePlayer,  [HarmonyArgument(1)] ref string chatText)
    {
        if (chatText.Contains("习近平") ||
            chatText.Contains("習近平") ||
            chatText.Contains("共产党") ||
            chatText.Contains("共產黨") ||
            chatText.Contains("国民党") ||
            chatText.Contains("國民黨") ||
            chatText.Contains("民国") ||
            chatText.Contains("民國") ||
            chatText.Contains("独立") ||
            chatText.Contains("獨立")
            )
        {
            int length = chatText.Length;
            chatText = "<color=#ff1919>";
            for (int i = 0; i < length; i++)
            {
                chatText += "█";
            }
            chatText += "</color>";
        }
        //SpamManager.CheckSpam(sourcePlayer, ref chatText);

    }
}

