using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinalSuspect_Xtreme.Modules;
using UnityEngine;
using YamlDotNet.Core;
using static FinalSuspect_Xtreme.Translator;
using static UnityEngine.GraphicsBuffer;
using System.Xml.Serialization;
using AmongUs.GameOptions;
using InnerNet;

namespace FinalSuspect_Xtreme;

[HarmonyPatch]
public static class MeetingHudPatch
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    [HarmonyPriority(Priority.First)]
    class StartPatch
    {
        public static void Prefix()
        {
            Logger.Info("------------会议开始------------", "Phase");
        }
        public static void Postfix(MeetingHud __instance)
        {

            foreach (var pva in __instance.playerStates)
            {
                var pc = Utils.GetPlayerById(pva.TargetPlayerId);
                if (pc == null) continue;
                pva.NameText.text = pc.GetTrueName();
                pva.ColorBlindName.transform.localPosition -= new Vector3(1.35f, 0f, 0f);

                var roleTextMeeting = UnityEngine.Object.Instantiate(pva.NameText);
                roleTextMeeting.text = "";
                roleTextMeeting.enabled = false;
                roleTextMeeting.transform.SetParent(pva.NameText.transform);
                roleTextMeeting.transform.localPosition = new Vector3(0f, -0.18f, 0f);
                roleTextMeeting.fontSize = 1.5f;
                roleTextMeeting.gameObject.name = "RoleTextMeeting";
                roleTextMeeting.enableWordWrapping = false;

                // 役職とサフィックスを同時に表示する必要が出たら要改修
                var suffixBuilder = new StringBuilder(32);
                var roleType = pc.Data.Role.Role;
                var dead = pc.Data.IsDead;
                var disconnected = pc.Data.Disconnected;

                PlayerData.AllPlayerData[pc.PlayerId].roleAfterDead = roleType;
                PlayerData.AllPlayerData[pc.PlayerId].Dead = dead;
                PlayerData.AllPlayerData[pc.PlayerId].Disconnected = disconnected;
                var name = PlayerData.AllPlayerData[pc.PlayerId].PlayerName;
                var color = Utils.GetRoleColorCode(roleType);

                if (pc == PlayerControl.LocalPlayer)
                {
                    pva.NameText.text =
    $"<color={color}>{pva.NameText.text}</color>";
                    suffixBuilder.Append
                        (
                        $"<color={color}><size=80%>{Translator.GetRoleString(roleType.ToString())}</size></color>"
                        );
                }
                else
                {

                    if ((PlayerControl.LocalPlayer.Data.IsDead && dead && PlayerControl.LocalPlayer.Data.Role.Role is RoleTypes.GuardianAngel)
                || PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.Data.Role.Role is not RoleTypes.GuardianAngel)
                    
                        {
                        pva.NameText.text =
                            $"<color={color}>{name}</color>";
                        suffixBuilder.Append
                        (
                            $"<color={color}><size=80%>{Translator.GetRoleString(roleType.ToString())}</size></color>");
                    }
                    else if (PlayerControl.LocalPlayer.IsImpostor() && pc.IsImpostor())
                    {
                        if (PlayerControl.LocalPlayer.Data.IsDead)
                        {
                            pva.NameText.text =
    $"<color=#ff1919>{name}</color>";
                            suffixBuilder.Append
                        (
                                   $"<color={color}><size=80%>{Translator.GetRoleString(roleType.ToString())}</size></color>");
                        }
                        else
                        {

                            pva.NameText.text =
                            $"<color=#FF1919>{name}</color>";

                        }
                    }
                    else
                    {

                        pva.NameText.text =
                        $"<color=#FFFFFF>{name}</color>";



                    }

                }
                if (suffixBuilder.Length > 0)
                {
                    roleTextMeeting.text = suffixBuilder.ToString() + $" {Utils.GetProgressText(pc)}";
                    roleTextMeeting.enabled = true;
                }

            }


        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    class UpdatePatch
    {
        public static void Prefix(MeetingHud __instance)
        {
            if (PlayerControl.LocalPlayer.Data.IsDead && !__instance.amDead)
            {
                __instance.SetForegroundForDead();
            }

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