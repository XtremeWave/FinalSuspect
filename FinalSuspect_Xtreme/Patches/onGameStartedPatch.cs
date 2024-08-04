using AmongUs.GameOptions;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace FinalSuspect_Xtreme;

public class PlayerData
{
    public static Dictionary<byte, PlayerData> AllPlayerData;
    public PlayerControl Player;
    public NetworkedPlayerInfo PlayerInfo;

    public string PlayerName;
    public int PlayerColor;

    public bool IsImpostor;
    public bool Dead;
    public bool Disconnected;

    public RoleTypes roleWhenAlive;
    public RoleTypes roleAfterDead;

    public int TotalTaskCount;
    public int CompleteTaskCount = 0;

    public PlayerData(
        PlayerControl player, 
        NetworkedPlayerInfo data,
        string playername, 
        int colorId, 
        bool isImp, 
        bool dead, 
        bool disconnected,
        RoleTypes roleWhenAlive,
        RoleTypes roleAfterDead)
    {
        Player = player;
        PlayerInfo = data;
        PlayerName = playername;
        PlayerColor = colorId;
        IsImpostor = isImp;
        Dead = dead;
        Disconnected = disconnected;
        this.roleWhenAlive = roleWhenAlive;
        this.roleAfterDead = roleAfterDead;
        CompleteTaskCount = 0;
        TotalTaskCount = 0;
    }
}
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
internal class CoStartGamePatch
{

    public static void Postfix(AmongUsClient __instance)
    {
        PlayerData.AllPlayerData = new();

        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            var colorId = pc.Data.DefaultOutfit.ColorId;
            var id = pc.PlayerId;
            var data = new PlayerData(pc, pc.Data, pc.GetTrueName(), colorId, false, false, false, RoleTypes.CrewmateGhost, RoleTypes.CrewmateGhost);
            PlayerData.AllPlayerData.Add(id, data);
        }

    }

}