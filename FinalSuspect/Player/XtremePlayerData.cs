using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using FinalSuspect.Attributes;

namespace FinalSuspect.Player;
public class XtremePlayerData : IDisposable
{
    #region PLAYER_INFO
    public static Dictionary<byte, XtremePlayerData> AllPlayerData; 
    public PlayerControl Player { get; private set; }
 
        public string Name { get; private set; }
        public int ColorId { get; private set; }

        public bool IsImpostor { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsDisconnected => RealDeathReason == DataDeathReason.Disconnect;


        public RoleTypes? RoleWhenAlive { get; private set; }
        public RoleTypes? RoleAfterDeath { get; private set; }
        public bool RoleAssgined { get; private set; }

        public DataDeathReason RealDeathReason { get; private set; }
        public XtremePlayerData RealKiller { get; private set; }

        
        public int ProcessInt { get; private set; }
        public int TotalTaskCount { get; private set; }
        public int CompleteTaskCount { get; private set; }
        public bool TaskCompleted => TotalTaskCount == CompleteTaskCount;
        public int KillCount { get; private set; }
        
        

        public XtremePlayerData(
            PlayerControl player,
            string playername,
            int colorid)
        {
            Player = player;
            Name = playername;
            ColorId = colorid;
            IsImpostor = IsDead = RoleAssgined = false;
            CompleteTaskCount = KillCount = TotalTaskCount = 0;
            RealDeathReason = DataDeathReason.None;
            RealKiller = null;
        }

    #endregion
    ///////////////FUNCTIONS\\\\\\\\\\\\\\\
    public static XtremePlayerData GetPlayerDataById(byte id) => AllPlayerData[id] ?? null;
    public static PlayerControl GetPlayerById(byte id) => GetPlayerDataById(id).Player ?? Utils.GetPlayerById(id);
    public static string GetPlayerNameById(byte id) => GetPlayerDataById(id).Name;

    public static RoleTypes GetRoleById(byte id)
    {
        var data = GetPlayerDataById(id);
        var dead = data.IsDead;
        RoleTypes role;
        RoleTypes nullrole;

        if (dead && !XtremeGameData.GameStates.IsFreePlay)
        {
            nullrole = data.IsImpostor ? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost;
        }
        else
        {
            nullrole = GetPlayerById(id).Data.Role.Role;
        }
            
        role = (dead ? data.RoleAfterDeath : data.RoleWhenAlive) ?? nullrole;            
        return role;
    }
    public static int GetLongestNameByteCount() => AllPlayerData.Values.Select(data => data.Name.GetByteCount()).OrderByDescending(byteCount => byteCount).FirstOrDefault();


    public void SetDead()
    {
        IsDead = true;
        Logger.Info($"Set Death For {Player.GetNameWithRole()}", "Data");
    }
    public void SetDisconnected()
    {
        Logger.Info($"Set Disconnect For {Player.GetNameWithRole()}", "Data");
        SetDead();
        SetDeathReason(DataDeathReason.Disconnect);
    }
    public void SetIsImp(bool isimp) => IsImpostor = isimp;
    public void SetRole(RoleTypes role)
    {
        if (!RoleAssgined)
        {
            RoleWhenAlive = role;
            SetIsImp(Utils.IsImpostor(role));
        }
        else
        {
            SetDead();
            RoleAfterDeath = role;
        }
        RoleAssgined = !XtremeGameData.GameStates.IsFreePlay;
    }
    public void SetDeathReason(DataDeathReason deathReason, bool focus = false)
    {
        if (IsDead && RealDeathReason == DataDeathReason.None || focus)
            RealDeathReason = deathReason;
        Logger.Info($"Set Death Reason For {Player.GetNameWithRole()}; Death Reason: {deathReason}", "Data");

    }
    public void SetRealKiller(XtremePlayerData killer)
    {
        SetDeathReason(DataDeathReason.Kill);
        killer.KillCount++;
        RealKiller = killer;
        Logger.Info($"Set Real Killer For {Player.GetNameWithRole()}, Killer: {killer.Player.GetNameWithRole()}, DeathReason:", "Data");
    }
    public void SetTaskTotalCount(int count) => TotalTaskCount = count;
    public void CompleteTask() => CompleteTaskCount++;



    [GameModuleInitializer]
    public static void InitializeAll()
    {
        AllPlayerData = new();
        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            var colorId = pc.Data.DefaultOutfit.ColorId;
            var id = pc.PlayerId;
            AllPlayerData[id] = new XtremePlayerData(pc, pc.GetRealName(), colorId);
        }
    }

    public static void CreateDataFor(PlayerControl player)
    {
        var colorId = player.Data.DefaultOutfit.ColorId;
        var id = player.PlayerId;
        AllPlayerData[id] = new XtremePlayerData(player, player.GetRealName(), colorId);
    }
#pragma warning disable CA1816
        public void Dispose()
        {
            if (XtremeGameData.GameStates.IsLobby) return;
            AllPlayerData.Remove(Player.PlayerId);
            Player = null;
            Name = null ;
            ColorId = -1 ;
            IsImpostor = IsDead = RoleAssgined = false;
            CompleteTaskCount = KillCount = TotalTaskCount = 0;
            RealDeathReason = DataDeathReason.None;
            RealKiller = null;
        }
#pragma warning restore CA1816
}
