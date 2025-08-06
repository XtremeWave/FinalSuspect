using AmongUs.GameOptions;
using FinalSuspect.DataHandling.FinalGameData;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using Object = UnityEngine.Object;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch]
public static class MeetingHudPatch
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
    public class UpdatePatch
    {
        public static void Postfix(MeetingHud __instance)
        {
            try
            {
                if (!__instance) return;
                if (AmongUsClient.Instance?.AmHost == true) return;

                for (var i = 0; i < __instance.playerStates?.Length; i++)
                {
                    var playerVoteArea = __instance.playerStates[i];
                    if (!playerVoteArea) continue;

                    var playerById = GameData.Instance?.GetPlayerById(playerVoteArea.TargetPlayerId);
                    if (!playerById)
                    {
                        playerVoteArea.SetDisabled();
                    }
                    else
                    {
                        var flag = playerById.Disconnected || playerById.IsDead;
                        if (flag == playerVoteArea.AmDead) continue;
                        var isReporter = __instance.reporterId == playerById.PlayerId;
                        playerVoteArea.SetDead(isReporter, flag,
                            playerById.Role?.Role == RoleTypes.GuardianAngel);
                        __instance.SetDirtyBit(1U);
                    }
                }
            }
            catch
            {
                /* ignored */
            }
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    [HarmonyPriority(Priority.First)]
    public class VotingCompletePatch
    {
        public static void Postfix([HarmonyArgument(1)] NetworkedPlayerInfo exiled, [HarmonyArgument(2)] bool tie)
        {
            foreach (var data in FinalPlayerData.AllPlayerData.Where(data => data?.Rend_DeadBody))
            {
                if (data == null) continue;
                Object.Destroy(data.Rend_DeadBody);
                data.Rend_DeadBody = null;
            }

            if (tie || !exiled) return;
            var player = GetPlayerById(exiled.PlayerId);
            player.SetDead();
            player.SetDeathReason(VanillaDeathReason.Exile, true);
        }
    }
}

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetHighlighted))]
internal class SetHighlightedPatch
{
    public static bool Prefix(PlayerVoteArea __instance, bool value)
    {
        if (!AmongUsClient.Instance.AmHost) return true;
        if (!__instance.HighlightedFX) return false;
        __instance.HighlightedFX.enabled = value;
        return false;
    }
}