using AmongUs.GameOptions;
using FinalSuspect.Helpers;
using UnityEngine;

namespace FinalSuspect.Modules.Core.Game.PlayerControlExtension;

public static class _Role
{
    public static RoleTypes GetRoleType(this PlayerControl player)
    {
        return Utils.GetRoleType(player.PlayerId);
    }

    public static bool IsImpostor(this PlayerControl pc)
    {
        return !IsLobby && pc.GetRoleType().IsImpostor();
    }

    public static string GetNameWithRole(this PlayerControl player, bool forUser = false)
    {
        var ret = $"{player?.Data?.PlayerName}{(IsInGame ?
            $"({GetRoleName(player.GetRoleType())})" : "")}";
        return forUser ? ret : ret.RemoveHtmlTags();
    }

    public static Color GetRoleColor(this PlayerControl player)
    {
        return Utils.GetRoleColor(player.GetRoleType());
    }
}