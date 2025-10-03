using AmongUs.GameOptions;
using FinalSuspect.Helpers;
using UnityEngine;

namespace FinalSuspect.Modules.Core.Game.PlayerControlExtension;

public static class _Role
{
    public static RoleTypes GetRoleType(this PlayerControl player)
    {
        return RoleHelper.GetRoleType(player.PlayerId);
    }

    public static bool IsImpostor(this PlayerControl pc)
    {
        return !IsLobby && RoleHelper.IsImpostor(pc.GetRoleType());
    }

    public static string GetNameWithRole(this PlayerControl player, bool forUser = false)
    {
        var ret = $"{player?.Data?.PlayerName}{(IsInGame ?
            $"({RoleHelper.GetRoleName(player.GetRoleType())})" : "")}";
        return forUser ? ret : ret.RemoveHtmlTags();
    }

    public static Color GetRoleColor(this PlayerControl player)
    {
        return RoleHelper.GetRoleColor(player.GetRoleType());
    }
}