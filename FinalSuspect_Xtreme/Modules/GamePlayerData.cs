using AmongUs.GameOptions;
using FinalSuspect_Xtreme.Attributes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.GraphicsBuffer;

namespace FinalSuspect_Xtreme;

public class GamePlayerData : IDisposable
{
    ///////////////PLAYER_INFO\\\\\\\\\\\\\\\
    public static Dictionary<byte, GamePlayerData> AllGamePlayerData;
    public PlayerControl Player { get; private set; }

    public string PlayerName { get; private set; }
    public int PlayerColor { get; private set; }

    public bool IsImpostor { get; private set; }
    public bool IsDead { get; private set; }
    public bool IsDisconnected { get; private set; }
    public bool Murdered { get; private set; }
    public bool Exiled { get; set; }


    public RoleTypes? RoleWhenAlive { get; private set; }
    public RoleTypes? RoleAfterDeath { get; private set; }
    public bool RoleAssgined { get; private set; }

    public DataDeathReason RealDeathReason { get; private set; }
    public GamePlayerData RealKiller { get; private set; }

    public int TotalTaskCount { get; private set; }
    public int CompleteTaskCount { get; private set; } = 0;
    public bool TaskCompleted
    {
        get
        {
            return TotalTaskCount == CompleteTaskCount;
        }
    }
    public int KillCount { get; private set; }

    ///////////////\\\\\\\\\\\\\\\

    public GamePlayerData(
    PlayerControl player,
    string playername,
    int colorId)
    {
        Player = player;
        PlayerName = playername;
        PlayerColor = colorId;
        IsImpostor = IsDead = Exiled = Murdered = IsDisconnected = RoleAssgined = false;
        CompleteTaskCount = KillCount = TotalTaskCount = 0;
        RealDeathReason = DataDeathReason.None;
        RealKiller = null;
    }

    [GameModuleInitializer]
    public static void Init()
    {
        AllGamePlayerData = new();
        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            var colorId = pc.Data.DefaultOutfit.ColorId;
            var id = pc.PlayerId;
            var data = new GamePlayerData(
                pc,
                pc.GetRealName(),
                colorId);
            AllGamePlayerData[id] = data;
        }

    }

    ///////////////FUNCTIONS\\\\\\\\\\\\\\\
    public static GamePlayerData GetPlayerDataById(byte id) => AllGamePlayerData[id] ?? null;
    public static PlayerControl GetPlayerById(byte id) => GetPlayerDataById(id).Player ?? Utils.GetPlayerById(id);
    public static string GetPlayerNameById(byte id) => GetPlayerDataById(id).PlayerName;

    public static RoleTypes GetRoleById(byte id) =>
        GetPlayerDataById(id).IsDead == true ? GetPlayerDataById(id).RoleAfterDeath ?? GetPlayerById(id).Data.Role.Role :
        GetPlayerDataById(id).RoleWhenAlive ?? GetPlayerById(id).Data.Role.Role;
    public static int GetLongestNameByteCount() => AllGamePlayerData.Values.Select(data => data.PlayerName.GetByteCount()).OrderByDescending(byteCount => byteCount).FirstOrDefault();


    public void SetDead() => IsDead = true;
    public void SetDisconnected()
    { 
        IsDisconnected = true;
        SetDead();
        SetDeathReason(DataDeathReason.Disconnect);
    }
    public void SetIsImp(bool isimp) => IsImpostor = isimp;
    public void SetRole(RoleTypes role)  
    {
        if (!RoleAssgined)
            RoleWhenAlive = role;
        else
            RoleAfterDeath = role;
        RoleAssgined = true;
    }
    public void SetDeathReason(DataDeathReason deathReason, bool focus = false)
    {
        if (RealDeathReason == DataDeathReason.None || focus)
            RealDeathReason = deathReason;
    }
    public void SetRealKiller(GamePlayerData killer)
    {
        killer.KillCount++;
        Murdered = true;
        RealKiller = killer;
    }
    public void SetTaskTotalCount(int TaskTotalCount) => TotalTaskCount = TaskTotalCount;
    public void CompleteTask() => CompleteTaskCount++;


#pragma warning disable CA1816
    public void Dispose()
    {
        AllGamePlayerData.Remove(Player.PlayerId);
        Player = null;
    }
#pragma warning restore CA1816
}
static class PlayerControlDataExtensions
{
    public static GamePlayerData GetPlayerData(this PlayerControl pc) => GamePlayerData.GetPlayerDataById(pc.PlayerId) ?? null;
    public static bool IsAlive(this PlayerControl pc) => GameStates.IsLobby || pc?.GetPlayerData()?.IsDead == false;
    public static string GetPlayerName(this PlayerControl pc) => GamePlayerData.GetPlayerNameById(pc.PlayerId);

    public static void SetDead(this PlayerControl pc) => pc.GetPlayerData().SetDead();
    public static void SetDisconnected(this PlayerControl pc) => pc.GetPlayerData().SetDisconnected();
    public static void SetIsImp(this PlayerControl pc, bool isimp) => pc.GetPlayerData().SetIsImp(isimp);
    public static void SetRole(this PlayerControl pc, RoleTypes role) => pc.GetPlayerData().SetRole(role);
    public static void SetDeathReason(this PlayerControl pc, DataDeathReason deathReason, bool focus = false)
        => pc.GetPlayerData().SetDeathReason(deathReason, focus);
    public static void SetRealKiller(this PlayerControl pc, PlayerControl killer)
    {
        pc.GetPlayerData().SetRealKiller(killer.GetPlayerData());
    }

    public static void SetTaskTotalCount(this PlayerControl pc, int TaskTotalCount) => pc.GetPlayerData().SetTaskTotalCount(TaskTotalCount);
    public static void OnCompleteTask(this PlayerControl pc) => pc.GetPlayerData().CompleteTask();
}


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
internal class DataFixedUpdate
{
    static bool Prefix(PlayerControl __instance)
    {
        if (!GameStates.IsInTask) return true;
        DisconnectSync(__instance);
        DeathSync(__instance);
        DeathReasonSync(__instance);
        return true;
    }
    static void DisconnectSync(PlayerControl pc)
    {
        var data = pc.GetPlayerData();
        var currectlyDisconnect = pc.Data.Disconnected && !data.IsDisconnected;
        var Task_NotAssgin = data.TotalTaskCount == 0;
        var Role_NotAssgin = data.RoleWhenAlive == null;

        if (currectlyDisconnect || Task_NotAssgin || Role_NotAssgin)
        {
            pc.SetDisconnected();
            pc.SetDeathReason(DataDeathReason.Disconnect, Task_NotAssgin || Role_NotAssgin);
        }
    }
    static void DeathSync(PlayerControl pc)
    {
        if (pc.Data.IsDead && !pc.GetPlayerData().IsDead) pc.SetDead();
    }
    static void DeathReasonSync(PlayerControl pc)
    {
        var data = pc.GetPlayerData();
        if (data.Exiled && data.RealDeathReason != DataDeathReason.Exile) pc.SetDeathReason(DataDeathReason.Exile, true);
        if (data.Murdered && data.RealDeathReason != DataDeathReason.Kill) pc.SetDeathReason(DataDeathReason.Kill, true);


    }
}
public enum DataDeathReason
{
    None,
    Exile,
    Kill,
    Disconnect
}