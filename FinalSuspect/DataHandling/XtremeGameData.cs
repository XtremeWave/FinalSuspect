using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using FinalSuspect.Modules.Core.Game;
using InnerNet;

namespace FinalSuspect.DataHandling;

public static class XtremeGameData
{
    public class PlayerVersion
    {
        public static Dictionary<byte, PlayerVersion> playerVersion = new();

        public readonly Version version;
        public readonly string tag;
        public readonly string forkId;

        public PlayerVersion(string ver, string tag_str, string forkId) : this(Version.Parse(ver), tag_str, forkId)
        {
        }

        public PlayerVersion(Version ver, string tag_str, string forkId)
        {
            version = ver;
            tag = tag_str;
            this.forkId = forkId;
        }

    }

    public static class GameStates
    {
        public static bool InGame { private get; set; } = false;

        public static bool OtherModHost
        {
            get
            {
                try
                {
                    return Main.AllPlayerControls.ToArray().FirstOrDefault(x => x.IsHost()
                        && !PlayerControl.LocalPlayer.IsHost()
                        && x.OtherModClient());
                }
                catch
                {
                    return false;
                }


            }
        }

        public static bool ModHost => Main.AllPlayerControls.ToArray().FirstOrDefault(x =>
            x.IsHost() && x.ModClient());

        public static bool IsLobby =>
            AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined && !IsFreePlay;

        public static bool IsInGame => InGame || IsFreePlay;
        public static bool IsNotJoined => AmongUsClient.Instance.GameState == InnerNetClient.GameStates.NotJoined;
        public static bool IsOnlineGame => AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;
        public static bool IsLocalGame => AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame;
        public static bool IsFreePlay => AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;
        public static bool IsInTask => IsInGame && !MeetingHud.Instance;
        public static bool IsMeeting => IsInGame && MeetingHud.Instance;

        public static bool IsCountDown => GameStartManager.InstanceExists &&
                                          GameStartManager.Instance.startState ==
                                          GameStartManager.StartingStates.Countdown;

        public static bool IsShip => ShipStatus.Instance != null;
        public static bool IsCanMove => PlayerControl.LocalPlayer?.CanMove is true;
        public static bool IsDead => PlayerControl.LocalPlayer?.Data?.IsDead is true && !IsLobby;

        public static bool IsNormalGame =>
            GameOptionsManager.Instance.CurrentGameOptions.GameMode is GameModes.Normal or GameModes.NormalFools;

        public static bool IsHideNSeek =>
            GameOptionsManager.Instance.CurrentGameOptions.GameMode is GameModes.HideNSeek or GameModes.SeekFools;

        public static bool MapIsActive(MapNames name)
        {
            return (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == name;
        }

        public static bool IsVanillaServer
        {
            get
            {
                if (!IsOnlineGame) return false;
                const string Domain = "among.us";
                //Reactor.gg
                return ServerManager.Instance.CurrentRegion?.TryCast<StaticHttpRegionInfo>() is { } regionInfo &&
                       regionInfo.PingServer.EndsWith(Domain, StringComparison.Ordinal) &&
                       regionInfo.Servers.All(serverInfo => serverInfo.Ip.EndsWith(Domain, StringComparison.Ordinal));
            }
        }
    }

    public static XtremePlayerData GetXtremeData(this PlayerControl pc)
    {
        try
        {
            return XtremePlayerData.GetXtremeDataById(pc.PlayerId);
        }
        catch
        {
            try
            {
                return XtremePlayerData.AllPlayerData.FirstOrDefault(data => data.Player == pc);
            }
            catch
            {
                return null;
            }
        }
       
    }

    public static bool IsAlive(this PlayerControl pc) => pc?.GetXtremeData()?.IsDead == false || !GameStates.IsInGame;

public static string GetDataName(this PlayerControl pc)
    {
        try
        {
            return XtremePlayerData.GetPlayerNameById(pc.PlayerId);
        }
        catch
        {
            return null;
        }
        
    }

    public static void SetDead(this PlayerControl pc) => pc.GetXtremeData().SetDead();
    public static void SetDisconnected(this PlayerControl pc) => pc.GetXtremeData().SetDisconnected();
    public static void SetRole(this PlayerControl pc, RoleTypes role) => pc.GetXtremeData().SetRole(role);
    public static void SetDeathReason(this PlayerControl pc, VanillaDeathReason deathReason, bool focus = false)
        => pc.GetXtremeData().SetDeathReason(deathReason, focus);
    public static void SetRealKiller(this PlayerControl pc, PlayerControl killer)
    {
        if (pc.GetXtremeData().RealKiller != null || !pc.Data.IsDead) return;
        pc.GetXtremeData().SetRealKiller(killer.GetXtremeData());
    }
    public static void SetTaskTotalCount(this PlayerControl pc, int TaskTotalCount) => pc.GetXtremeData().SetTaskTotalCount(TaskTotalCount);
    public static void OnCompleteTask(this PlayerControl pc) => pc.GetXtremeData().CompleteTask();
    public static bool OtherModClient(this PlayerControl player) => OtherModClient(player.PlayerId) || player.Data.OwnerId == -2 && !IsFinalSuspect(player.PlayerId);
    public static bool OtherModClient(byte id) => PlayerVersion.playerVersion.TryGetValue(id, out var ver) && Main.ForkId != ver.forkId;

    public static bool ModClient(this PlayerControl player) => ModClient(player.PlayerId);
    public static bool ModClient(byte id) => PlayerVersion.playerVersion.ContainsKey(id);
    
    public static bool IsFinalSuspect(this PlayerControl pc)  => IsFinalSuspect(pc.PlayerId);

    public static bool IsFinalSuspect(byte id) =>
        PlayerVersion.playerVersion.TryGetValue(id, out var ver) && Main.ForkId == ver.forkId;

}

public enum VanillaDeathReason
{
    None,
    Exile,
    Kill,
    Disconnect
}