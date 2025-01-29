using System;
using System.Collections;
using System.Linq;
using System.Text;
using AmongUs.Data;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils;
using FinalSuspect.Attributes;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Patches.System;
using FinalSuspect.Templates;
using InnerNet;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch(typeof(Vent), nameof(Vent.SetOutline))]
class SetVentOutlinePatch
{
    public static void Postfix(Vent __instance, [HarmonyArgument(1)] ref bool mainTarget)
    {
        XtremeLocalHandling.SetVentOutlineColor(__instance, ref mainTarget);
    }
}
[HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.SetTaskText))]
class TaskPanelBehaviourPatch
{
    public static void Postfix(TaskPanelBehaviour __instance)
    {
        if (!XtremeGameData.GameStates.IsInGame) return;

        var player = PlayerControl.LocalPlayer;
        var role = player.GetRoleType();
        var taskText = __instance.taskText.text;
        if (taskText == "None") return;


        var RoleWithInfo = $"{Utils.GetRoleName(role)}:\r\n";
        RoleWithInfo += role.GetRoleInfoForVanilla();

        var AllText = StringHelper.ColorString(player.GetRoleColor(), RoleWithInfo);


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
                text = $"{StringHelper.ColorString(new Color32(255, 20, 147, byte.MaxValue), GetString("FakeTask"))}\r\n{text}";
            AllText += $"\r\n\r\n<size=85%>{text}</size>";
        }

        AllText += $"\r\n\r\n</color><size=70%>{GetString("PressF1ShowRoleDescription")}</size>";

        __instance.taskText.text = AllText;

    }
}

public static class HudManagerPatch
{
    static GameObject ModLoading;


    private static int currentIndex;
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
                __instance.StartCoroutine(SwitchRoleIllustration(Sprite));
            }
            //Scrapped
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

        public static void Postfix(HudManager __instance)
        {
            try
            {
                UpdateResult(__instance);
                SetChatBG(__instance);
                //SetAbilityButtonColor(__instance);
            }
            catch 
            {
                //
            }
            
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
    public static IEnumerator SwitchRoleIllustration(SpriteRenderer spriter)
    {
        while (true)
        {
            if (AwakeAccountManager.AllRoleRoleIllustration.Length == 0) yield break;

            spriter.sprite = AwakeAccountManager.AllRoleRoleIllustration[currentIndex];
            var p = 1f;
            while (p > 0f)
            {
                p -= Time.deltaTime * 2.8f;
                var alpha = 1 - p;
                spriter.color = Color.white.AlphaMultiplied(alpha);
                yield return null;
            }
            currentIndex = (currentIndex + 1) % AwakeAccountManager.AllRoleRoleIllustration.Length;


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

    private static TextMeshPro roleSummary;
    public static SimpleButton showHideButton;
    private static SpriteRenderer backgroundRenderer;

    [GameModuleInitializer]
    public static void Init()
    {
        try
        {
            Object.Destroy(showHideButton.Button.gameObject);
            Object.Destroy(roleSummary.gameObject);
            Object.Destroy(backgroundRenderer.gameObject); // 销毁背景
        }
        catch { }
        showHideButton = null;
        roleSummary = null;
        backgroundRenderer = null;
    }

    
    public static void SetChatBG(HudManager __instance)
    {
        Color color;
        if (XtremeGameData.GameStates.IsInGame)
        {
            if (PlayerControl.LocalPlayer.IsImpostor())
            {
                color = Utils.GetRoleColor(PlayerControl.LocalPlayer.GetRoleType());
            }
            else
            {
                color = Utils.GetRoleColor(RoleTypes.Crewmate);
            }
        }
        else
        {
            color = ColorHelper.TeamColor32;
        }

        __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("Background").gameObject
            .GetComponent<SpriteRenderer>().color = color;
    }
    public static void SetAbilityButtonColor(HudManager __instance)
    {
        if (!XtremeGameData.GameStates.IsInGame || PlayerControl.LocalPlayer.GetRoleType() is RoleTypes.Crewmate or RoleTypes.Impostor or RoleTypes.CrewmateGhost or RoleTypes.ImpostorGhost)   return;
        var color = Utils.GetRoleColor(PlayerControl.LocalPlayer.GetRoleType());
        __instance.AbilityButton.buttonLabelText.color = color;
    }
    public static int GetLineCount(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;
        var lines = text.Split(["\r\n", "\n"], StringSplitOptions.None);
        return lines.Length;
    }
    public static string LastResultText;
    public static string LastGameData;
    public static string LastGameResult;
    public static string LastRoomCode;
    public static string LastServer;

    [GameModuleInitializer]
    public static void InitForLastResult()
    {
        LastResultText = LastGameData = LastGameResult = LastRoomCode = LastServer = "";
    }
    public static void UpdateResult(HudManager __instance)
    {
        if (XtremeGameData.GameStates.IsFreePlay || !XtremeGameData.GameStates.IsInGame && GetLineCount(LastResultText) < 6 )
            return;
        var showInitially = Main.ShowResults.Value;
       
        if (showHideButton == null)
        {
            showHideButton =
            new SimpleButton(
               __instance.transform,
               "ShowHideResultsButton",
               XtremeGameData.GameStates.IsInGame? new(0.2f, 2.685f, -14f) : new(-4.5f, 2.6f, -14f),  // 比 BackgroundLayer(z = -13) 更靠前
               new(209, 190, 255, byte.MaxValue),
               new(208, 222, 255, byte.MaxValue),
               () =>
               {
                   var setToActive = !roleSummary.gameObject.activeSelf;
                   roleSummary.gameObject.SetActive(setToActive);
                   Main.ShowResults.Value = setToActive;
                   showHideButton.Label.text = GetString(setToActive ? "HideResults" : "ShowResults");
               },
               GetString(showInitially ? "HideResults" : "ShowResults"))
            {
                Scale = new(1.5f, 0.5f),
                FontSize = 2f,
            };
        }
        

        StringBuilder sb = new($"{GetString("RoleSummaryText")}{LastGameResult}");
        if (XtremeGameData.GameStates.IsInGame)
        {
            LastRoomCode = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
            LastServer = XtremeGameData.GameStates.IsOnlineGame
                ? PingTrackerUpdatePatch.ServerName
                : GetString("Local");
        }
            
        var gamecode =  StringHelper.ColorString(
            ColorHelper.ModColor32, 
            DataManager.Settings.Gameplay.StreamerMode?  new string('*', LastRoomCode.Length): LastRoomCode);
        sb.Append("\n"+ LastServer +"  "+gamecode);
        if (XtremeGameData.GameStates.IsInGame)
        {
            StringBuilder sb2 = new();
            foreach (var data in XtremePlayerData.AllPlayerData)
            {
                sb2.Append("\n\u3000 ").Append(Utils.SummaryTexts(data.PlayerId));
            }

            LastGameData = sb2.ToString();
        }

        sb.Append(LastGameData);
        LastResultText = sb.ToString();
        if (roleSummary == null)
        {

            roleSummary = TMPTemplate.Create(
                "RoleSummaryText",
                LastResultText,
                Color.white,
                1.25f,
                TextAlignmentOptions.TopLeft,
                setActive: showInitially,
                parent: showHideButton.Button.transform);
            roleSummary.transform.localPosition = new Vector3(1.7f, -0.4f, -1f);
            roleSummary.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            roleSummary.fontStyle = FontStyles.Bold;
            roleSummary.SetOutlineColor(Color.black);
            roleSummary.SetOutlineThickness(0.15f);
 
            var backgroundObject = new GameObject("RoleSummaryBackground");
            backgroundObject.transform.SetParent(roleSummary.transform); 
            backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
            backgroundRenderer.sprite = Utils.LoadSprite("LastResult-BG.png",200f);
            backgroundRenderer.color = new(0.5f,0.5f,0.5f,1f); 
        }
        
        showHideButton.Button.transform.localPosition =
            XtremeGameData.GameStates.IsInGame ? new(0.2f, 2.685f, -14f) : new(-4.5f, 2.6f, -1f);
        if (XtremeGameData.GameStates.IsInGame)
            showHideButton.Button.gameObject.SetActive
            (PlayerControl.LocalPlayer.GetRoleType() is RoleTypes.CrewmateGhost or RoleTypes.ImpostorGhost &&
             !XtremeGameData.GameStates.IsMeeting);
        else
            showHideButton.Button.gameObject.SetActive(true);

        roleSummary.text = LastResultText;
        AdjustBackgroundSize();
    }

    private static void AdjustBackgroundSize()
    {
        if (roleSummary != null && backgroundRenderer != null)
        {
            var textBounds = roleSummary.textBounds;

            var backgroundSprite = backgroundRenderer.sprite;
            if (backgroundSprite != null)
            {
                var scaleX = (textBounds.size.x + 0.4f) / backgroundSprite.bounds.size.x;
                var scaleY = (textBounds.size.y + 0.5f) / backgroundSprite.bounds.size.y;
                
                backgroundRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
                backgroundRenderer.transform.localPosition = new Vector3(textBounds.center.x, textBounds.center.y,2f);
            }
        }
    }
    
}