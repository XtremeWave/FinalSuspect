using AmongUs.GameOptions;
using FinalSuspect.Attributes;
using FinalSuspect.Patches;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.GraphicsBuffer;

namespace FinalSuspect;

public static class XtremeGameData
{

    public class XtremePlayerData : IDisposable
    {
        ///////////////PLAYER_INFO\\\\\\\\\\\\\\\
        public static Dictionary<byte, XtremePlayerData> AllPlayerData;
        public PlayerControl Player { get; private set; }

        public string PlayerName { get; private set; }
        public int PlayerColor { get; private set; }

        public bool IsImpostor { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsDisconnected => RealDeathReason == DataDeathReason.Disconnect;


        public RoleTypes? RoleWhenAlive { get; private set; }
        public RoleTypes? RoleAfterDeath { get; private set; }
        public bool RoleAssgined { get; private set; }

        public DataDeathReason RealDeathReason { get; private set; }
        public XtremePlayerData RealKiller { get; private set; }

        public int TotalTaskCount { get; private set; }
        public int CompleteTaskCount { get; private set; }
        public bool TaskCompleted => TotalTaskCount == CompleteTaskCount;
        public int KillCount { get; private set; }

        ///////////////\\\\\\\\\\\\\\\

        public XtremePlayerData(
        PlayerControl player,
        string playername,
        int colorId)
        {
            Player = player;
            PlayerName = playername;
            PlayerColor = colorId;
            IsImpostor = IsDead = RoleAssgined = false;
            CompleteTaskCount = KillCount = TotalTaskCount = 0;
            RealDeathReason = DataDeathReason.None;
            RealKiller = null;
        }


        ///////////////FUNCTIONS\\\\\\\\\\\\\\\
        public static XtremePlayerData GetPlayerDataById(byte id) => AllPlayerData[id] ?? null;
        public static PlayerControl GetPlayerById(byte id) => GetPlayerDataById(id).Player ?? Utils.GetPlayerById(id);
        public static string GetPlayerNameById(byte id) => GetPlayerDataById(id).PlayerName;

        public static RoleTypes GetRoleById(byte id) =>
            GetPlayerDataById(id).IsDead == true ?
            GetPlayerDataById(id).RoleAfterDeath ?? GetPlayerById(id).Data.Role.Role :
            GetPlayerDataById(id).RoleWhenAlive ?? GetPlayerById(id).Data.Role.Role;
        public static int GetLongestNameByteCount() => AllPlayerData.Values.Select(data => data.PlayerName.GetByteCount()).OrderByDescending(byteCount => byteCount).FirstOrDefault();


        public void SetDead() => IsDead = true;
        public void SetDisconnected()
        {
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
            RoleAssgined = !GameStates.IsFreePlay;
        }
        public void SetDeathReason(DataDeathReason deathReason, bool focus = false)
        {
            if (IsDead && RealDeathReason == DataDeathReason.None || focus)
                RealDeathReason = deathReason;
        }
        public void SetRealKiller(XtremePlayerData killer)
        {
            SetDead();
            SetDeathReason(DataDeathReason.Kill);
            killer.KillCount++;
            RealKiller = killer;
        }
        public void SetTaskTotalCount(int TaskTotalCount) => TotalTaskCount = TaskTotalCount;
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
#pragma warning disable CA1816
        public void Dispose()
        {
            if (GameStates.IsLobby) return;
            AllPlayerData.Remove(Player.PlayerId);
            Player = null;
            PlayerName = null ;
            PlayerColor = -1 ;
            IsImpostor = IsDead = RoleAssgined = false;
            CompleteTaskCount = KillCount = TotalTaskCount = 0;
            RealDeathReason = DataDeathReason.None;
            RealKiller = null;
        }
#pragma warning restore CA1816
    }

    public class PlayerVersion
    {
        public static Dictionary<byte, PlayerVersion> playerVersion = new();

        public readonly Version version;
        public readonly string tag;
        public readonly string forkId;
        public PlayerVersion(string ver, string tag_str, string forkId) : this(Version.Parse(ver), tag_str, forkId) { }
        public PlayerVersion(Version ver, string tag_str, string forkId)
        {
            version = ver;
            tag = tag_str;
            this.forkId = forkId;
        }

    }
    public static class GameStates
    {
        public static bool InGame = false;
        public static bool OtherModHost => Main.AllPlayerControls.ToArray().FirstOrDefault(x => 
        x.OwnerId == AmongUsClient.Instance.HostId && x.OtherModClient());
        public static bool ModHost => Main.AllPlayerControls.ToArray().FirstOrDefault(x =>
x.OwnerId == AmongUsClient.Instance.HostId && x.ModClient());
        public static bool IsLobby => AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Joined && !IsFreePlay;
        public static bool IsInGame => InGame || IsFreePlay;
        public static bool IsNotJoined => AmongUsClient.Instance.GameState == AmongUsClient.GameStates.NotJoined;
        public static bool IsOnlineGame => AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;
        public static bool IsLocalPlayerGame => AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame;
        public static bool IsFreePlay => AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;
        public static bool IsInTask => IsInGame && !MeetingHud.Instance;
        public static bool IsMeeting => IsInGame && MeetingHud.Instance;
        public static bool IsCountDown => GameStartManager.InstanceExists && GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown;
        public static bool IsShip => ShipStatus.Instance != null;
        public static bool IsCanMove => PlayerControl.LocalPlayer?.CanMove is true;
        public static bool IsDead => PlayerControl.LocalPlayer?.Data?.IsDead is true;
        public static bool IsNormalGame => GameOptionsManager.Instance.CurrentGameOptions.GameMode is GameModes.Normal or GameModes.NormalFools;
        public static bool IsHideNSeek => GameOptionsManager.Instance.CurrentGameOptions.GameMode is GameModes.HideNSeek or GameModes.SeekFools;        
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
                // From Reactor.gg
                return ServerManager.Instance.CurrentRegion?.TryCast<StaticHttpRegionInfo>() is { } regionInfo &&
                       regionInfo.PingServer.EndsWith(Domain, StringComparison.Ordinal) &&
                       regionInfo.Servers.All(serverInfo => serverInfo.Ip.EndsWith(Domain, StringComparison.Ordinal));
            }
        }
    }




    public static XtremePlayerData GetPlayerData(this PlayerControl pc) => XtremePlayerData.GetPlayerDataById(pc.PlayerId) ?? null;
    public static bool IsAlive(this PlayerControl pc) => GameStates.IsLobby || pc?.GetPlayerData()?.IsDead == false;
    public static string GetDataName(this PlayerControl pc) => XtremePlayerData.GetPlayerNameById(pc.PlayerId);
    public static void SetDead(this PlayerControl pc) => pc.GetPlayerData().SetDead();
    public static void SetDisconnected(this PlayerControl pc) => pc.GetPlayerData().SetDisconnected();
    public static void SetIsImp(this PlayerControl pc, bool isimp) => pc.GetPlayerData().SetIsImp(isimp);
    public static void SetRole(this PlayerControl pc, RoleTypes role) => pc.GetPlayerData().SetRole(role);
    public static void SetDeathReason(this PlayerControl pc, DataDeathReason deathReason, bool focus = false)
        => pc.GetPlayerData().SetDeathReason(deathReason, focus);
    public static void SetRealKiller(this PlayerControl pc, PlayerControl killer)
    {
        if (pc.GetPlayerData().RealKiller != null || !pc.Data.IsDead) return;
        pc.GetPlayerData().SetRealKiller(killer.GetPlayerData());
    }
    public static void SetTaskTotalCount(this PlayerControl pc, int TaskTotalCount) => pc.GetPlayerData().SetTaskTotalCount(TaskTotalCount);
    public static void OnCompleteTask(this PlayerControl pc) => pc.GetPlayerData().CompleteTask();
    public static bool OtherModClient(this PlayerControl player) => OtherModClient(player.PlayerId);
    public static bool OtherModClient(byte id) => PlayerVersion.playerVersion.TryGetValue(id, out var ver) && Main.ForkId != ver.forkId;

    public static bool ModClient(this PlayerControl player) => ModClient(player.PlayerId);
    public static bool ModClient(byte id) => PlayerVersion.playerVersion.ContainsKey(id);

}
public enum DataDeathReason
{
    None,
    Exile,
    Kill,
    Disconnect
}