using AmongUs.GameOptions;
using FinalSuspect.Modules.Core.Game;
using UnityEngine;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch]
public static class MeetingHudPatch
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
    class UpdatePatch
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (AmongUsClient.Instance.AmHost) return;
            for (var i = 0; i < __instance.playerStates.Length; i++)
            {
                var playerVoteArea = __instance.playerStates[i];
                var playerById = GameData.Instance.GetPlayerById(playerVoteArea.TargetPlayerId);
                if (playerById == null)
                {
                    playerVoteArea.SetDisabled();
                }
                else
                {
                    var flag = playerById.Disconnected || playerById.IsDead;
                    if (flag != playerVoteArea.AmDead)
                    {
                        playerVoteArea.SetDead(__instance.reporterId == playerById.PlayerId, flag, playerById.Role.Role == RoleTypes.GuardianAngel);
                        __instance.SetDirtyBit(1U);
                    }
                }
            }

        }
        
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    [HarmonyPriority(Priority.First)]
    class VotingCompletePatch
    {
        public static void Postfix(
            [HarmonyArgument(1)]NetworkedPlayerInfo exiled, 
            [HarmonyArgument(2)]bool tie )
        {
            foreach (var data in XtremePlayerData.AllPlayerData)
            {
                if (data.deadbodyrend)
                    GameObject.Destroy(data.deadbodyrend);
                data.deadbodyrend = null;
            }
            if (tie || exiled == null) return;
            var player = Utils.GetPlayerById(exiled.PlayerId);
            player.SetDead();
            player.SetDeathReason(VanillaDeathReason.Exile, true);
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