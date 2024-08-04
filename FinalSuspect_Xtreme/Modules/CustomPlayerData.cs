using AmongUs.GameOptions;
using FinalSuspect_Xtreme.Attributes;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FinalSuspect_Xtreme;

public class CustomPlayerData
{
    ///////////////PLAYER_INFO\\\\\\\\\\\\\\\
    public static Dictionary<byte, CustomPlayerData> AllCustomPlayerData;
    public PlayerControl Player { get; private set; }
    public NetworkedPlayerInfo PlayerInfo { get; private set; }

    public string PlayerName { get; private set; }
    public int PlayerColor { get; private set; }

    public bool IsImpostor { get; private set; }
    public bool IsDead { get; private set; }
    public bool IsDisconnected { get; private set; }

    public RoleTypes RoleWhenAlive{ get; private set; }
    public RoleTypes RoleAfterDeath { get; private set; }
    public bool RoleAssgined { get; private set; }

    public int TotalTaskCount { get; private set; }
    public int CompleteTaskCount { get; private set; } = 0;
    public bool TaskCompleted 
    { 
        get
        {
            return TotalTaskCount == CompleteTaskCount;
        }
    }
    ///////////////\\\\\\\\\\\\\\\

    public CustomPlayerData(
    PlayerControl player,
    NetworkedPlayerInfo data,
    string playername,
    int colorId)
    {
        Player = player;
        PlayerInfo = data;
        PlayerName = playername;
        PlayerColor = colorId;
        IsImpostor = false;
        IsDead = false;
        IsDisconnected = false;
        RoleWhenAlive = RoleTypes.CrewmateGhost;
        RoleAfterDeath = RoleTypes.CrewmateGhost;
        RoleAssgined = false;
        CompleteTaskCount = 0;
        TotalTaskCount = 0;
    }
    [GameModuleInitializer]
    public static void Init()
    {
        AllCustomPlayerData = new();
        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            var colorId = pc.Data.DefaultOutfit.ColorId;
            var id = pc.PlayerId;
            var data = new CustomPlayerData(
                pc,
                pc.Data,
                pc.GetTrueName(),
                colorId);
            AllCustomPlayerData.Add(id, data);
        }

    }

    ///////////////FUNCTIONS\\\\\\\\\\\\\\\
    public static CustomPlayerData GetPlayerDataById(byte id) => AllCustomPlayerData[id] ?? null;
    public static PlayerControl GetPlayerById(byte id) => GetPlayerDataById(id).Player ?? null;
    public static string GetPlayerNameById(byte id) => GetPlayerDataById(id).PlayerName;

    public static RoleTypes GetRoleById(byte id) => GetPlayerDataById(id).IsDead? GetPlayerDataById(id).RoleAfterDeath : GetPlayerDataById(id).RoleWhenAlive;
    public static int GetLongestNameByteCount() => AllCustomPlayerData.Values.Select(data => data.PlayerName.GetByteCount()).OrderByDescending(byteCount => byteCount).FirstOrDefault();


    public void SetInfo(NetworkedPlayerInfo info) => PlayerInfo ??= info;
    public void SetDead() => IsDead = true;
    public void SetDisconnected() => IsDisconnected = true;
    public void SetIsImp(bool isimp) => IsImpostor = isimp;

    public void SetRole(RoleTypes role)  
    {
        if (RoleAssgined)
            RoleWhenAlive = role;
        else
            RoleAfterDeath = role;

        RoleAssgined = true;
    }
    public void SetTaskTotalCount(int TaskTotalCount) => TotalTaskCount = TaskTotalCount;
    public void CompleteTask() => CompleteTaskCount++;



}
static class PlayerControlDataExtensions
{
    public static CustomPlayerData GetPlayerData(this PlayerControl pc) => CustomPlayerData.GetPlayerDataById(pc.PlayerId);
    public static bool IsAlive(this PlayerControl pc) => pc.GetPlayerData().IsDead == false;
    public static string GetPlayerName(this PlayerControl pc) => CustomPlayerData.GetPlayerNameById(pc.PlayerId);

}