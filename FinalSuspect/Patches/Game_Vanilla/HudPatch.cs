using System;
using System.Collections;
using System.Text;
using AmongUs.Data;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils;
using FinalSuspect.Attributes;
using FinalSuspect.DataHandling.FinalGameData;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using FinalSuspect.Modules.Features.DisplayedRoleTag;
using FinalSuspect.Patches.System;
using FinalSuspect.Templates;
using InnerNet;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch(typeof(Vent), nameof(Vent.SetOutline))]
internal class SetVentOutlinePatch
{
    public static void Postfix(Vent __instance, [HarmonyArgument(1)] ref bool mainTarget)
    {
        FinalLocalHandling.SetVentOutlineColor(__instance, ref mainTarget);
    }
}

[HarmonyPatch(typeof(TaskPanelBehaviour), nameof(TaskPanelBehaviour.SetTaskText))]
internal class TaskPanelBehaviourPatch
{
    private static bool even;

    public static void Postfix(TaskPanelBehaviour __instance)
    {
        if (!IsInGame) return;

        var player = PlayerControl.LocalPlayer;
        var role = player.GetRoleType();
        var taskText = __instance.taskText.text;
        if (taskText == "None") return;

        var RoleWithInfo = $"{GetRoleName(role)}:\r\n";
        RoleWithInfo += role.GetRoleInfoForVanilla();

        var AllText = StringHelper.ColorString(player.GetRoleColor(), RoleWithInfo);

        if (!taskText.Contains(GetString(StringNames.FixComms)) || PlayerControl.LocalPlayer.IsImpostor())
        {
            var lines = taskText.Split("\r\n</color>\n")[0].Split("\r\n\n")[0].Split("\r\n");
            StringBuilder sb = new();
            foreach (var eachLine in lines)
            {
                var line = eachLine.Trim();
                if (((line.StartsWith("<color=#FF1919FF>") || line.StartsWith("<color=#FF0000FF>")) && sb.Length < 1 &&
                     !line.Contains('(')) || line.Contains(GetString(StringNames.FixComms))) continue;
                sb.Append(line + "\r\n");
            }

            if (sb.Length > 1)
            {
                var text = sb.ToString().TrimEnd('\n').TrimEnd('\r');
                if (player.IsImpostor() && sb.ToString().Count(s => s == '\n') >= 2)
                    text =
                        $"{StringHelper.ColorString(new Color32(255, 20, 147, byte.MaxValue), GetString("FakeTask"))}\r\n{text}";
                AllText += $"\r\n\r\n<size=85%>{text}</size>";
            }
        }

        if (taskText.Contains(GetString(StringNames.FixComms)))
        {
            even = !even;
            var color = even ? Color.yellow : Color.red;
            var text = color.ToTextColor();
            text += GetString(StringNames.FixComms);
            text += "</color>";
            AllText += $"\r\n\r\n<size=85%>{text}</size>";
        }

        AllText += $"\r\n\r\n</color><size=70%>{GetString("PressF1ShowRoleDescription")}</size>";

        __instance.taskText.text = AllText;
    }
}

public static class HudManagerPatch
{
    private static GameObject ModLoading;

    private static int currentIndex;

    private static TextMeshPro roleSummary;
    public static SimpleButton showHideButton;
    private static SpriteRenderer backgroundRenderer;

    private static bool Refresh;

    private static IEnumerator SwitchRoleIllustration(SpriteRenderer spriter)
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

    [GameModuleInitializer]
    public static void Init()
    {
        try
        {
            Object.Destroy(showHideButton.Button.gameObject);
            Object.Destroy(roleSummary.gameObject);
            Object.Destroy(backgroundRenderer.gameObject); //销毁背景
        }
        catch
        {
            /* ignored */
        }

        showHideButton = null;
        roleSummary = null;
        backgroundRenderer = null;
    }

    private static void SetChatBG(HudManager __instance)
    {
        Color color;
        if (IsInGame)
        {
            if (PlayerControl.LocalPlayer.IsImpostor())
                color = ColorHelper.ImpostorRedPale;
            else
                color = GetRoleColor(RoleTypes.Crewmate);
        }
        else
        {
            color = ColorHelper.TeamColor;
        }

        __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("Background").gameObject
            .GetComponent<SpriteRenderer>().color = color;
    }

    [GameModuleInitializer]
    public static void InitForRefresh()
    {
        Refresh = false;
    }

    private static void SetAbilityButtonColor(HudManager __instance)
    {
        if (!IsInGame)
            return;
        var color = GetRoleColor(PlayerControl.LocalPlayer.GetRoleType());
        __instance.AbilityButton.buttonLabelText.SetOutlineColor(color);
        __instance.AbilityButton.cooldownTimerText.color = color;
        __instance.KillButton.cooldownTimerText.color = ColorHelper.ImpostorRedPale;

        // 刷新按钮状态
        if (!__instance.AbilityButton.gameObject.active || Refresh) return;
        Refresh = true;
        __instance.AbilityButton.gameObject.SetActive(false);
        __instance.AbilityButton.gameObject.SetActive(true);
    }

    private static int GetLineCount(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;
        var lines = text.Split(["\r\n", "\n"], StringSplitOptions.None);
        return lines.Length;
    }

    private static void UpdateResult(HudManager __instance)
    {
        if (IsFreePlay || (!IsInGame && GetLineCount(FinalGameData.LastResultText) < 6))
            return;
        var showInitially = Main.ShowResults.Value;

        showHideButton ??=
            new SimpleButton(
                __instance.transform,
                "ShowHideResultsButton",
                IsInGame
                    ? new Vector3(0.2f * GetResolutionOffset(), 2.685f, -14f)
                    : new Vector3(-4.5f * GetResolutionOffset(), 2.6f, -14f), // 比 BackgroundLayer(z = -13) 更靠前
                new Color32(209, 190, 255, byte.MaxValue),
                new Color32(208, 222, 255, byte.MaxValue),
                () =>
                {
                    var setToActive = !roleSummary.gameObject.activeSelf;
                    roleSummary.gameObject.SetActive(setToActive);
                    Main.ShowResults.Value = setToActive;
                    showHideButton.Label.text = GetString(setToActive ? "Summary.HideResults" : "Summary.ShowResults");
                },
                GetString(showInitially ? "Summary.HideResults" : "Summary.ShowResults"))
            {
                Scale = new Vector2(1.5f, 0.5f),
                FontSize = 2f
            };

        StringBuilder sb = new($"{GetString("Summary.Text")}{FinalGameData.LastGameResult}");
        if (IsInGame)
        {
            FinalGameData.LastRoomCode = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
            FinalGameData.LastServer = IsOnlineGame
                ? PingTrackerUpdatePatch.ServerName
                : GetString("Local");
        }

        var gamecode = StringHelper.ColorString(
            ColorHelper.FSColor,
            DataManager.Settings.Gameplay.StreamerMode
                ? new string('*', FinalGameData.LastRoomCode.Length)
                : FinalGameData.LastRoomCode);
        sb.Append("\n" + FinalGameData.LastServer + "  " + gamecode);
        if (IsInGame)
        {
            StringBuilder sb2 = new();
            foreach (var data in FinalPlayerData.AllPlayerData)
                sb2.Append("\n\u3000 ").Append(SummaryTexts(data.PlayerId));

            FinalGameData.LastGameData = sb2.ToString();
        }

        sb.Append(FinalGameData.LastGameData);
        FinalGameData.LastResultText = sb.ToString();
        if (!roleSummary)
        {
            roleSummary = TMPTemplate.Create(
                "RoleSummaryText", FinalGameData.LastResultText,
                Color.white,
                1.25f,
                TextAlignmentOptions.TopLeft,
                showInitially,
                showHideButton.Button.transform);
            roleSummary.transform.localPosition =
                new Vector3(IsInGame ? 0f : 1.7f, -0.4f, -1f);
            roleSummary.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            roleSummary.fontStyle = FontStyles.Bold;
            roleSummary.SetOutlineColor(Color.black);
            roleSummary.SetOutlineThickness(0.15f);

            var backgroundObject = new GameObject("RoleSummaryBackground");
            backgroundObject.transform.SetParent(roleSummary.transform);
            backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
            backgroundRenderer.sprite = LoadSprite("LastResult-BG.png", 200f);
            backgroundRenderer.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        showHideButton.Button.transform.localPosition =
            IsInGame ? new Vector3(0.2f, 2.685f, -14f) : new Vector3(-4.5f, 2.6f, -1f);
        if (IsInGame)
        {
            showHideButton.Button.gameObject.SetActive
            (PlayerControl.LocalPlayer.GetRoleType() is RoleTypes.CrewmateGhost or RoleTypes.ImpostorGhost &&
             !IsInMeeting);
        }
        else
            showHideButton.Button.gameObject.SetActive(true);

        roleSummary.text = FinalGameData.LastResultText;
        AdjustBackgroundSize();
    }

    private static void AdjustBackgroundSize()
    {
        if (!roleSummary || !backgroundRenderer) return;
        var textBounds = roleSummary.textBounds;

        var backgroundSprite = backgroundRenderer.sprite;
        if (!backgroundSprite) return;
        var scaleX = (textBounds.size.x + 0.4f) / backgroundSprite.bounds.size.x;
        var scaleY = (textBounds.size.y + 0.5f) / backgroundSprite.bounds.size.y;

        backgroundRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        backgroundRenderer.transform.localPosition = new Vector3(textBounds.center.x, textBounds.center.y, 2f);
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class Update
    {
        public static void Prefix(HudManager __instance)
        {
            if (!ModLoading)
            {
                ModLoading = new GameObject("ModLoading") { layer = 5 };
                ModLoading.transform.SetParent(__instance.GameLoadAnimation.transform.parent);

                var Sprite = ModLoading.AddComponent<SpriteRenderer>();
                Sprite.color = Color.white;
                Sprite.flipX = false;
                ModLoading.SetActive(false);
                __instance.StartCoroutine(SwitchRoleIllustration(Sprite));

                var ap = ModLoading.AddComponent<AspectPosition>();
                ap.Alignment = AspectPosition.EdgeAlignments.RightBottom;
                ap.DistanceFromEdge = new Vector3(0.6f, 0.5f, -1000);
                ap.updateAlways = true;

                ModLoading.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            }

            ModLoading.SetActive(!IsInGame && !IsLobby);

            var ap_n = __instance.Notifier.GetComponent<AspectPosition>();
            ap_n.DistanceFromEdge = new Vector3(ap_n.DistanceFromEdge.x, ap_n.DistanceFromEdge.y, -900);

            //ModLogo.SetActive(!IsInGame && !IsLobby);
            /*Scrapped
            if (WarningText == null)
            {
                WarningText = Object.Instantiate(__instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("TextArea").gameObject, __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField"));
                var tmp = WarningText.GetComponent<TextMeshPro>();
                tmp.text = GetString("BrowsingMode");
                tmp.color = Color.blue;
                WarningText.SetActive(false);

            }
            if (IsInGame)
            {
                if (IsInTask)
                {
                    if (PlayerControl.LocalPlayer.IsAlive())
                    {
                        __instance.Chat.gameObject.SetActive(true);
                        WarningText.SetActive(true);
                        __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("Background").gameObject.SetActive(false);
                        __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("CharCounter (TMP)").gameObject.SetActive(false);
                        __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("ChatSendButton").gameObject.SetActive(false);
                        __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("TextArea").gameObject.SetActive(false);
                        __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("QuickChatPreview").gameObject.SetActive(false);
                    }
                    else
                    {
                        WarningText.SetActive(false);
                        __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("Background").gameObject.SetActive(true);
                        __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("CharCounter (TMP)").gameObject.SetActive(true);
                        __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("ChatSendButton").gameObject.SetActive(true);
                        __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("TextArea").gameObject.SetActive(true);

                    }
                }
                else if (!__instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").gameObject.active && !__instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("QuickChatPreview").gameObject.active)
                {
                    WarningText.SetActive(false);
                    __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("Background").gameObject.SetActive(true);
                    __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("CharCounter (TMP)").gameObject.SetActive(true);
                    __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("ChatSendButton").gameObject.SetActive(true);
                    __instance.Chat.chatScreen.transform.FindChild("ChatScreenContainer").FindChild("FreeChatInputField").FindChild("TextArea").gameObject.SetActive(true);
                }
            }*/
        }

        public static void Postfix(HudManager __instance)
        {
            try
            {
                UpdateResult(__instance);
                SetChatBG(__instance);
                SetAbilityButtonColor(__instance);

                if (!IsFreePlay && IsInGame)
                {
                    var notShowPane = DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening ||
                                      DisplayerRoleTagHelper.selectionUI != null &&
                                      DisplayerRoleTagHelper.selectionUI.activeSelf ||
                                      !ControllerManagerUpdatePatch.ShowSettingsPanel ||
                                      !FinalGameData.IntroDestroyed ||
                                      !ControllerManagerUpdatePatch.ShowHudUI;

                    if (GameStartManagerPatch.Instance.LobbyInfoPane.gameObject.activeSelf && notShowPane)
                    {
                        GameStartManagerPatch.Instance.LobbyInfoPane.DeactivatePane();
                    }

                    GameStartManagerPatch.Instance.LobbyInfoPane.gameObject.SetActive(!notShowPane);
                }
            }
            catch
            {
                /* ignored */
            }
        }
    }
}