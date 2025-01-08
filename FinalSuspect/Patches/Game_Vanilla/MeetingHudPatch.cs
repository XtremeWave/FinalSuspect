using AmongUs.GameOptions;
using FinalSuspect.Modules.Core.Game;

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
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    [HarmonyPriority(Priority.First)]
    class VotingCompletePatch
    {
        public static void Postfix(
            [HarmonyArgument(1)]NetworkedPlayerInfo exiled, 
            [HarmonyArgument(2)]bool tie )
        {
            if (tie || exiled == null) return;
            var player = Utils.GetPlayerById(exiled.PlayerId);
            player.SetDead();
            player.SetDeathReason(DataDeathReason.Exile, true);
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