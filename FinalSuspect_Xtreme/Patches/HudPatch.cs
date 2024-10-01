using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;
using static FinalSuspect_Xtreme.Translator;
using TMPro;
using InnerNet;

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
    public static void Postfix(TaskPanelBehaviour __instance)
    {
        if (!XtremeGameData.GameStates.IsInGame) return;

        PlayerControl player = PlayerControl.LocalPlayer;
        var role = player.GetRoleType();
        var taskText = __instance.taskText.text;
        if (taskText == "None") return;


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

    }
}

public static class HudManagerPatch
{
    static GameObject ModLoading = null;


    private static int currentIndex = 0;
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class Update
    {
        public static void Prefix(HudManager __instance)
        {
            if (ModLoading == null && !XtremeGameData.GameStates.IsFreePlay)
            {
                ModLoading = new GameObject("ModLoading") { layer = 5 };
                ModLoading.transform.SetParent(__instance.GameLoadAnimation.transform.parent);

                ModLoading.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
                ModLoading.transform.localPosition = new Vector3(4.5833f, -2.25f, -600);

                var Sprite = ModLoading.AddComponent<SpriteRenderer>();
                Sprite.color = Color.white;
                Sprite.flipX = false;
                ModLoading.SetActive(false);
                __instance.StartCoroutine(SwitchCharacterIllustration(Sprite));
            }
            //if (WarningText == null)
            //{
            //    WarningText = Object.Instantiate(__instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("TextArea").gameObject, __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField"));
            //    var tmp = WarningText.GetComponent<TextMeshPro>();
            //    tmp.text = GetString("BrowsingMode");
            //    tmp.color = Color.blue;
            //    WarningText.SetActive(false);

            //}
            //if (XtremeGameData.GameStates.IsInGame)
            //{
            //    if (XtremeGameData.GameStates.IsInTask)
            //    {
            //        if (PlayerControl.LocalPlayer.IsAlive())
            //        {
            //            __instance.Chat.gameObject.SetActive(true);
            //            WarningText.SetActive(true);
            //            __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("Background").gameObject.SetActive(false);
            //            __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("CharCounter (TMP)").gameObject.SetActive(false);
            //            __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("ChatSendButton").gameObject.SetActive(false);
            //            __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("TextArea").gameObject.SetActive(false);
            //            __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("QuickChatPreview").gameObject.SetActive(false);
            //        }
            //        else
            //        {
            //            WarningText.SetActive(false);
            //            __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("Background").gameObject.SetActive(true);
            //            __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("CharCounter (TMP)").gameObject.SetActive(true);
            //            __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("ChatSendButton").gameObject.SetActive(true);
            //            __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("TextArea").gameObject.SetActive(true);

            //        }
            //    }
            //    else if (!__instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").gameObject.active && !__instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("QuickChatPreview").gameObject.active)
            //    {
            //        WarningText.SetActive(false);
            //        __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("Background").gameObject.SetActive(true);
            //        __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("CharCounter (TMP)").gameObject.SetActive(true);
            //        __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("ChatSendButton").gameObject.SetActive(true);
            //        __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("TextArea").gameObject.SetActive(true);
            //    }
            //}
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.CoFadeFullScreen))]
    public static class CoFadeFullScreen
    {
        public static void Prefix([HarmonyArgument(3)] ref bool showLoader)
        {
            ModLoading.SetActive(showLoader);
            showLoader = false;
        }

    }
    public static IEnumerator SwitchCharacterIllustration(SpriteRenderer spriter)
    {
        while (true)
        {
            if (AwakeAccountManager.AllRoleCharacterIllustration.Length == 0) yield break;

            spriter.sprite = AwakeAccountManager.AllRoleCharacterIllustration[currentIndex];
            var p = 1f;
            while (p > 0f)
            {
                p -= Time.deltaTime * 2.8f;
                float alpha = 1 - p;
                spriter.color = Color.white.AlphaMultiplied(alpha);
                yield return null;
            }
            currentIndex = (currentIndex + 1) % AwakeAccountManager.AllRoleCharacterIllustration.Length;


            yield return new WaitForSeconds(1f);
            p = 1f;
            while (p > 0f)
            {
                p -= Time.deltaTime * 2.8f;
                spriter.color = Color.white.AlphaMultiplied(p);
                yield return null;
            }
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.HideGameLoader))]
    public static class HideGameLoader
    {
        static void Prefix()
        {
            ModLoading.SetActive(false);
        }
    }
}
