using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using FinalSuspect.Attributes;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game;

namespace FinalSuspect.DataHandling;
public class XtremePlayerData : IDisposable
{
    #region PLAYER_INFO
    public static List<XtremePlayerData> AllPlayerData; 
    public PlayerControl Player { get; private set; }
 
        public string Name { get; private set; }
        public int ColorId { get; private set; }
        public byte PlayerId { get; private set; }

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
            PlayerId = player.PlayerId;
            IsImpostor = IsDead = RoleAssgined = false;
            CompleteTaskCount = KillCount = TotalTaskCount = 0;
            RealDeathReason = DataDeathReason.None;
            RealKiller = null;
        }

    #endregion
    ///////////////FUNCTIONS\\\\\\\\\\\\\\\
    
    public static XtremePlayerData GetXtremeDataById(byte id)
    {
        try
        {
            return AllPlayerData.Where(data => data.PlayerId == id).FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }
    public static PlayerControl GetPlayerById(byte id) => GetXtremeDataById(id).Player ?? null;
    public static string GetPlayerNameById(byte id) => GetXtremeDataById(id).Name;

    public static RoleTypes GetRoleById(byte id)
    {
        var data = GetXtremeDataById(id);
        var dead = data?.IsDead ?? false;
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

    public void AdjustPlayerId()
    {
        PlayerId = Player.PlayerId;
    }
    public static int GetLongestNameByteCount() => AllPlayerData.Select(data => data.Name.GetByteCount()).OrderByDescending(byteCount => byteCount).FirstOrDefault();

public void SetName(string name) => Name = name;
    public void SetDead()
    {
        IsDead = true;
        XtremeLogger.Info($"Set Death For {Player.GetNameWithRole()}", "Data");
    }
    public void SetDisconnected()
    {
        if (XtremeGameData.GameStates.IsLobby)
        {
            Dispose();
            AllPlayerData.Remove(this);
            return;
        }
        XtremeLogger.Info($"Set Disconnect For {Player.GetNameWithRole()}", "Data");
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
        XtremeLogger.Info($"Set Death Reason For {Player.GetNameWithRole()}; Death Reason: {deathReason}", "Data");

    }
    public void SetRealKiller(XtremePlayerData killer)
    {
        SetDead();
        SetDeathReason(DataDeathReason.Kill);
        killer.KillCount++;
        RealKiller = killer;
        XtremeLogger.Info($"Set Real Killer For {Player.GetNameWithRole()}, Killer: {killer.Player.GetNameWithRole()}, DeathReason:", "Data");
    }
    public void SetTaskTotalCount(int count) => TotalTaskCount = count;
    public void CompleteTask() => CompleteTaskCount++;



    [GameModuleInitializer]
    public static void InitializeAll()
    {
        AllPlayerData = new();
        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            CreateDataFor(pc);
        }
    }

    public static void CreateDataFor(PlayerControl player, string playername = null)
    {
        var colorId = player.Data.DefaultOutfit.ColorId;
        AllPlayerData.Add(new XtremePlayerData(player, playername ?? player.GetRealName(), colorId));
    }
#pragma warning disable CA1816
    public void Dispose()
    {
        XtremeLogger.Info($"Disposing XtremePlayerData For {Name}", "Data");
        Player = null;
        Name = null;
        ColorId = -1;
        IsImpostor = IsDead = RoleAssgined = false;
        CompleteTaskCount = KillCount = TotalTaskCount = 0;
        RealDeathReason = DataDeathReason.None;
        RealKiller = null;
    }

    public static void DisposeAll()
    {
        AllPlayerData.Do(data => data.Dispose());
        AllPlayerData.Clear();
    }
#pragma warning restore CA1816
}
