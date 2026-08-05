using System;
using System.Text;
using AmongUs.GameOptions;
using FinalSuspect.Attributes;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using FinalSuspect.Modules.Core.Game.UI;

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
    private static bool _even;

    public static void Postfix(TaskPanelBehaviour __instance)
    {
        if (!IsInGame) return;

        var player = PlayerControl.LocalPlayer;
        var role = player.GetRoleType();
        var taskText = __instance.taskText.text;
        if (taskText == "None") return;

        var RoleWithInfo = $"{RoleHelper.GetRoleName(role)}:\r\n";
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
            _even = !_even;
            var color = _even ? Color.yellow : Color.red;
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
    private static bool Refresh;

    private static void SetChatBG(HudManager __instance)
    {
        Color color;
        if (IsInGame)
        {
            if (PlayerControl.LocalPlayer.IsImpostor())
                color = ColorHelper.ImpostorRedPale;
            else
                color = RoleHelper.GetRoleColor(RoleTypes.Crewmate);
        }
        else
        {
            color = ColorHelper.AuthorColor;
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
        var role = PlayerControl.LocalPlayer.GetRoleType();
        var color = RoleHelper.GetRoleColor(role);
        __instance.AbilityButton.buttonLabelText.SetOutlineColor(color);
        __instance.AbilityButton.cooldownTimerText.color = color;
        __instance.SecondaryAbilityButton.buttonLabelText.SetOutlineColor(color);
        __instance.SecondaryAbilityButton.cooldownTimerText.color = color;

        __instance.KillButton.cooldownTimerText.color = ColorHelper.ImpostorRedPale;

        // 刷新按钮状态
        if (Refresh) return;
        Refresh = true;
        var active1 = __instance.AbilityButton.gameObject.active;
        __instance.AbilityButton.gameObject.SetActive(false);
        __instance.AbilityButton.gameObject.SetActive(active1);
        var active2 = __instance.SecondaryAbilityButton.gameObject.active;
        __instance.SecondaryAbilityButton.gameObject.SetActive(false);
        __instance.SecondaryAbilityButton.gameObject.SetActive(active2);
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public static class HudManagerStartPatch
    {
        public static void Postfix(HudManager __instance)
        {
            var notifier_aspectPosition = __instance.Notifier.GetComponent<AspectPosition>();
            notifier_aspectPosition.DistanceFromEdge += Vector3.back * 900;
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class Update
    {
        public static void Prefix(HudManager __instance)
        {
            RoleIllustrationManager.Create(__instance);


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
                LastResult.UpdateResult(__instance);
                SetChatBG(__instance);
                SetAbilityButtonColor(__instance);
                InGameInfoPane.SetShowInfoPanel();
            }
            catch (Exception e)
            {
                Error(e.Message, "Update");
                /* ignored */
            }
        }
    }
}