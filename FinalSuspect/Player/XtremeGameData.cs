using AmongUs.GameOptions;
using FinalSuspect.Patches;
using HarmonyLib;
using MS.Internal.Xml.XPath;
using System;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.GraphicsBuffer;

namespace FinalSuspect.Player
{
    public static class XtremeGameData
    {
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
            public static bool IsLocalGame => AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame;
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
}

namespace FinalSuspect
{
    public enum DataDeathReason
    {
        None,
        Exile,
        Kill,
        Disconnect
    }
}