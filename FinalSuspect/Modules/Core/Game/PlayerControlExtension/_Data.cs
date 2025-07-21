using AmongUs.GameOptions;
using FinalSuspect.DataHandling.FinalAntiCheat.Core;
using FinalSuspect.DataHandling.FinalGameData;
using FinalSuspect.Helpers;
using UnityEngine;

namespace FinalSuspect.Modules.Core.Game.PlayerControlExtension;

public static class _Data
{
    public static string GetRealName(this PlayerControl player, bool isMeeting = false)
    {
        if (player == null) return null;

        string dataName = null;
        try
        {
            var data = player.GetFinalData();
            if (data != null)
                dataName = player.GetDataName();
        }
        catch
        {
            /* ignored */
        }

        var realName = isMeeting ? player.Data?.PlayerName : player.name;
        return realName ?? dataName;
    }

    public static string CheckAndGetNameWithDetails(
        this PlayerControl player,
        out Color topcolor,
        out Color bottomcolor,
        out string toptext,
        out string bottomtext,
        bool topswap = false)
    {
        return FinalLocalHandling.CheckAndGetNameWithDetails(player.PlayerId, out topcolor, out bottomcolor,
            out toptext, out bottomtext,
            topswap);
    }

    public static FinalPlayerData GetFinalData(this PlayerControl pc)
    {
        try
        {
            return GetFinalDataById(pc.PlayerId);
        }
        catch
        {
            try
            {
                return FinalPlayerData.AllPlayerData.FirstOrDefault(data => data.Player == pc);
            }
            catch
            {
                return null;
            }
        }
    }

    public static PlayerCheatData GetCheatData(this PlayerControl pc)
    {
        try
        {
            return GetCheatDataById(pc.PlayerId);
        }
        catch
        {
            try
            {
                return pc.GetFinalData().CheatData;
            }
            catch
            {
                return null;
            }
        }
    }

    public static string GetDataName(this PlayerControl pc)
    {
        try
        {
            return GetPlayerNameById(pc.PlayerId);
        }
        catch
        {
            return null;
        }
    }

    public static string GetColoredName(this PlayerControl pc)
    {
        try
        {
            var data = GetFinalDataById(pc.PlayerId);
            return StringHelper.ColorString(Palette.PlayerColors[data.ColorId], data.Name);
        }
        catch
        {
            return null;
        }
    }

    public static void SetDead(this PlayerControl pc)
    {
        pc.GetFinalData().SetDead();
    }

    public static void SetDisconnected(this PlayerControl pc)
    {
        pc.GetFinalData().SetDisconnected();
        FinalPlayerData.AllPlayerData.Do(_data => _data.AdjustPlayerId());
    }

    public static void SetRole(this PlayerControl pc, RoleTypes role)
    {
        pc.GetFinalData().SetRole(role);
    }

    public static void SetDeathReason(this PlayerControl pc, VanillaDeathReason deathReason, bool focus = false)
    {
        pc.GetFinalData().SetDeathReason(deathReason, focus);
    }

    public static void SetRealKiller(this PlayerControl pc, PlayerControl killer)
    {
        if (pc.GetFinalData().RealKiller != null || !pc.Data.IsDead) return;
        pc.GetFinalData().SetRealKiller(killer.GetFinalData());
    }

    public static void SetTaskTotalCount(this PlayerControl pc, int TaskTotalCount)
    {
        pc.GetFinalData().SetTaskTotalCount(TaskTotalCount);
    }
}