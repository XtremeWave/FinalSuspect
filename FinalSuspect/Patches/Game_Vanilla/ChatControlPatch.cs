using System.Collections.Generic;
using AmongUs.Data;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using InnerNet;
using UnityEngine;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch(typeof(ChatController))]


[HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
public static class ChatControllerUpdatePatch
{
    public static int CurrentHistorySelection = -1;
    public static void Prefix()
    {

        if (AmongUsClient.Instance.AmHost && DataManager.Settings.Multiplayer.ChatMode == QuickChatModes.QuickChatOnly)
            DataManager.Settings.Multiplayer.ChatMode = QuickChatModes.FreeChatOrQuickChat; //コマンドを打つためにホストのみ常時フリーチャット開放

    }
    public static void Postfix(ChatController __instance)
    {
        if (XtremeGameData.GameStates.OtherModHost || XtremeGameData.GameStates.IsFreePlay || XtremeGameData.GameStates.IsLocalGame)
        {
            __instance.timeSinceLastMessage = 3f;
        }
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
    public static void Prefix([HarmonyArgument(1)] ref string chatText)
    {
        SpamManager.CheckSpam(ref chatText);
    }

    public static void Postfix(ChatController __instance)
    {
        var name = __instance.chatNotification.playerNameText.text;
        SpamManager.CheckSpam(ref name);
        __instance.chatNotification.playerNameText.text = name;
    }
}

