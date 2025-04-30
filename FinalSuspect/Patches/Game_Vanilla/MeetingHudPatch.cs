using AmongUs.GameOptions;
using Object = UnityEngine.Object;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch]
public static class MeetingHudPatch
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
    class UpdatePatch
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (__instance == null) return; // 空值检查
            if (AmongUsClient.Instance?.AmHost == true) return; // 确保 Instance 和 AmHost 不为空

            for (var i = 0; i < __instance.playerStates?.Length; i++) // 确保数组不为空
            {
                var playerVoteArea = __instance.playerStates[i];
                if (playerVoteArea == null) continue; // 跳过空值

                var playerById = GameData.Instance?.GetPlayerById(playerVoteArea.TargetPlayerId);
                if (playerById == null)
                {
                    playerVoteArea.SetDisabled();
                }
                else
                {
                    var flag = playerById.Disconnected || playerById.IsDead;
                    if (flag != playerVoteArea.AmDead)
                    {
                        var isReporter = __instance.reporterId == playerById.PlayerId; 
                        playerVoteArea.SetDead(isReporter, flag, 
                            playerById.Role?.Role == GuardianAngel);
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
        public static void Postfix([HarmonyArgument(1)]NetworkedPlayerInfo exiled, [HarmonyArgument(2)]bool tie )
        {
            foreach (var data in XtremePlayerData.AllPlayerData)
            {
                if (data?.Deadbodyrend != null)
                {
                    Object.Destroy(data.Deadbodyrend);
                    data.Deadbodyrend = null;
                }
            }
            if (tie || exiled == null) return;
            var player = GetPlayerById(exiled.PlayerId);
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