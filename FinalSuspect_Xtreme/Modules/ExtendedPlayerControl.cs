using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinalSuspect_Xtreme.Modules;
using UnityEngine;
using static FinalSuspect_Xtreme.Translator;

using FinalSuspect_Xtreme.Attributes;


namespace FinalSuspect_Xtreme;

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
        if (player != null)
        {
            return  player?.Data?.Role?.Role ?? GetRoleType(player.PlayerId);
        }
        return RoleTypes.Crewmate;
    }
    public static RoleTypes GetRoleType(byte id)
    {
        return Utils.GetPlayerById(id)?.Data?.Role?.Role ?? PlayerData.AllPlayerData[id].roleWhenAlive;
    }
    public static bool IsImpostor(this PlayerControl pc)
    {
        switch (pc.GetRoleType())
        {
            case RoleTypes.Impostor:
            case RoleTypes.Shapeshifter:
            case RoleTypes.Phantom:
            case RoleTypes.ImpostorGhost:
                return true;
        }
        return false;
    }
    public static bool IsImpostor(byte id) => IsImpostor(Utils.GetPlayerById(id));

    /*public static GameOptionsData DeepCopy(this GameOptionsData opt)
    {
        var optByte = opt.ToBytes(5);
        return GameOptionsData.FromBytes(optByte);
    }*/

    public static string GetNameWithRole(this PlayerControl player, bool forUser = false)
    {
        var ret = $"{player?.Data?.PlayerName}" + (GameStates.IsInGame? $"({Utils.GetRoleName(player.GetRoleType())})" : "");
        return (forUser ? ret : ret.RemoveHtmlTags());
    }
    public static string GetRoleColorCode(this PlayerControl player)
    {
        return Utils.GetRoleColorCode(player.GetRoleType());
    }
    public static Color GetRoleColor(this PlayerControl player)
    {
        return Utils.GetRoleColor(player.GetRoleType());
    }

    public static string GetTrueName(this PlayerControl player)
    {
        return Main.AllPlayerNames.TryGetValue(player.PlayerId, out var name) ? name : GetRealName(player, GameStates.IsMeeting);
    }
    public static string GetRealName(this PlayerControl player, bool isMeeting = false)
    {
        return isMeeting ? player?.Data?.PlayerName : player?.name;
    }
    public static bool IsModClient(this PlayerControl player) => Main.playerVersion.ContainsKey(player.PlayerId);

    public static bool IsAlive(this PlayerControl target)
    {

        return !target.Data.Disconnected && !target.Data.IsDead;
    }
    
}