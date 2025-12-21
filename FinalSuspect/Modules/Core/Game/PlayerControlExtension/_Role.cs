using AmongUs.GameOptions;
using FinalSuspect.Helpers;

namespace FinalSuspect.Modules.Core.Game.PlayerControlExtension;

public static class _Role
{
    extension(PlayerControl player)
    {
        public RoleTypes GetRoleType()
        {
            return RoleHelper.GetRoleType(player.PlayerId);
        }

        public bool IsImpostor()
        {
            return !IsLobby && player.GetRoleType().IsImpostor();
        }

        public string GetNameWithRole(bool forUser = false)
        {
            var ret = $"{player?.Data?.PlayerName}{(IsInGame ?
                $"({RoleHelper.GetRoleName(player.GetRoleType())})" : "")}";
            return forUser ? ret : ret.RemoveHtmlTags();
        }

        public Color GetRoleColor()
        {
            return RoleHelper.GetRoleColor(player.GetRoleType());
        }
    }
}