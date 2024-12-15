using HarmonyLib;
using UnityEngine;
using AmongUs.GameOptions;
using FinalSuspect.Modules.Managers;

namespace FinalSuspect;

[HarmonyPatch]
public static class MeetingHudPatch
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    [HarmonyPriority(Priority.First)]
    class StartPatch
    {
        public static void Postfix(MeetingHud __instance)
        {

            foreach (var pva in __instance.playerStates)
            {
                pva.ColorBlindName.transform.localPosition -= new Vector3(1.35f, 0f, 0f);

                var pc = Utils.GetPlayerById(pva.TargetPlayerId);
                var text = pc.GetDataName();
                SpamManager.CheckSpam(ref text);
                pva.NameText.text = text;

                var roleTextMeeting = Object.Instantiate(pva.NameText);
                roleTextMeeting.text = "";
                roleTextMeeting.enabled = false;
                roleTextMeeting.transform.SetParent(pva.NameText.transform);
                roleTextMeeting.transform.localPosition = new Vector3(0f, -0.18f, 0f);
                roleTextMeeting.fontSize = 1.5f;
                roleTextMeeting.gameObject.name = "RoleTextMeeting";
                roleTextMeeting.enableWordWrapping = false;


                pc.GetGameText(out string color, out bool appendText, out string roleText);

                if (appendText)
                {
                    if (!PlayerControl.LocalPlayer.IsAlive())
                        pva.NameText.text += Utils.GetVitalText(pva.TargetPlayerId, docolor: PlayerControl.LocalPlayer.GetRoleType() != RoleTypes.GuardianAngel);
                }

                pva.NameText.text = $"<color={color}>{pva.NameText.text}</color>";

                if (roleText.Length > 0)
                {
                    roleTextMeeting.text = roleText;
                    roleTextMeeting.enabled = true;
                }

                if (pc.GetPlayerData().IsDisconnected)
                {
                    color = "#" + ColorHelper.ColorToHex(Color.gray);
                    pva.NameText.text = $"<color={color}>{pva.NameText.text}</color>";
                }

            }


        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
    class UpdatePatch
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (AmongUsClient.Instance.AmHost) return;
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                NetworkedPlayerInfo playerById = GameData.Instance.GetPlayerById(playerVoteArea.TargetPlayerId);
                if (playerById == null)
                {
                    playerVoteArea.SetDisabled();
                }
                else
                {
                    bool flag = playerById.Disconnected || playerById.IsDead;
                    if (flag != playerVoteArea.AmDead)
                    {
                        playerVoteArea.SetDead(__instance.reporterId == playerById.PlayerId, flag, playerById.Role.Role == RoleTypes.GuardianAngel);
                        __instance.SetDirtyBit(1U);
                    }
                }
            }

        }
    }
}
[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetHighlighted))]
class SetHighlightedPatch
{
    public static bool Prefix(PlayerVoteArea __instance, bool value)
    {
        if (!AmongUsClient.Instance.AmHost) return true;
        if (!__instance.HighlightedFX) return false;
        __instance.HighlightedFX.enabled = value;
        return false;
    }
}