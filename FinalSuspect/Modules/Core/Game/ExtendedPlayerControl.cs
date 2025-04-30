using System.Linq;
using AmongUs.GameOptions;
using FinalSuspect.Helpers;
using InnerNet;
using UnityEngine;

namespace FinalSuspect.Modules.Core.Game;

static class ExtendedPlayerControl
{
    public static ClientData GetClient(this PlayerControl player)
    {
        try
        {
            var client = AmongUsClient.Instance.allClients.ToArray().Where(cd => cd.Character.PlayerId == player.PlayerId).FirstOrDefault();
            return client;
        }
        catch
        {
            return null;
        }
    }
    public static int GetClientId(this PlayerControl player)
    {
        if (player == null) return -1;
        var client = player.GetClient();
        return client == null ? -1 : client.Id;
    }
    public static RoleTypes GetRoleType(this PlayerControl player)
    {
        return GetRoleType(player.PlayerId);
    }
    public static RoleTypes GetRoleType(byte id)
    {
        return XtremePlayerData.GetRoleById(id);
    }
    public static bool IsImpostor(this PlayerControl pc)
    {
        if (IsLobby) return false;
        return pc.GetRoleType() switch
        {
            Impostor or Shapeshifter or Phantom or ImpostorGhost => true,
            _ => false,
        };
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
    public static string GetRealName(this PlayerControl player, bool isMeeting = false)
    {
        string trynull = null;
        try
        {
            trynull = player.GetXtremeData() != null ? player?.GetDataName() : null;
        }
        catch
        {
            // ignored
        }

        var nullname = trynull;
        return (isMeeting ? player?.Data?.PlayerName : player?.name) ?? nullname;
    }
    public static bool IsLocalPlayer(this PlayerControl player) => PlayerControl.LocalPlayer == player;
    public static bool IsHost(this PlayerControl player)
    {
        try
        {
            return  AmongUsClient.Instance.GetHost().Id == player.GetClient().Id;
        }
        catch 
        {
            return false;
        }
    }
}