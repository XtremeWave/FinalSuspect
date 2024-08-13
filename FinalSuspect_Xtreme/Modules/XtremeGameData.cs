using AmongUs.GameOptions;
using FinalSuspect_Xtreme.Attributes;
using FinalSuspect_Xtreme.Patches;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.GraphicsBuffer;

namespace FinalSuspect_Xtreme;

public static class XtremeGameData
{

    public class PlayerData : IDisposable
    {
        ///////////////PLAYER_INFO\\\\\\\\\\\\\\\
        public static Dictionary<byte, PlayerData> AllPlayerData;
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
        public PlayerData RealKiller { get; private set; }

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

        public PlayerData(
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


        ///////////////FUNCTIONS\\\\\\\\\\\\\\\
        public static PlayerData GetPlayerDataById(byte id) => AllPlayerData[id] ?? null;
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
            if (IsDead && RealDeathReason == DataDeathReason.None || focus)
                RealDeathReason = deathReason;
        }
        public void SetRealKiller(PlayerData killer)
        {
            SetDead();
            SetDeathReason(DataDeathReason.Kill);
            killer.KillCount++;
            Murdered = true;
            RealKiller = killer;

        }
        public void SetTaskTotalCount(int TaskTotalCount) => TotalTaskCount = TaskTotalCount;
        public void CompleteTask() => CompleteTaskCount++;



        [GameModuleInitializer]
        public static void Init()
        {
            AllPlayerData = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                var colorId = pc.Data.DefaultOutfit.ColorId;
                var id = pc.PlayerId;
                var data = new PlayerData(
                    pc,
                    pc.GetRealName(),
                    colorId);
                AllPlayerData[id] = data;
            }

        }
#pragma warning disable CA1816
        public void Dispose()
        {
            AllPlayerData.Remove(Player.PlayerId);
            Player = null;
        }
#pragma warning restore CA1816
    }

    public class PlayerVersion
    {
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
        public bool IsEqual(PlayerVersion pv)
        {
            return pv.version == version && pv.tag == tag;
        }
    }
    public static class GameStates
    {
        public static bool InGame = false;
        public static bool AlreadyDied = false;
        public static bool IsModHost => Main.AllPlayerControls.ToArray().FirstOrDefault(x => x.OwnerId == AmongUsClient.Instance.HostId && x.IsModClient());
        public static bool IsLobby => AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Joined && !IsFreePlay;
        public static bool IsInGame => InGame || IsFreePlay;
        public static bool IsEnded => AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Ended;
        public static bool IsNotJoined => AmongUsClient.Instance.GameState == AmongUsClient.GameStates.NotJoined;
        public static bool IsOnlineGame => AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;
        public static bool IsLocalPlayerGame => AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame;
        public static bool IsFreePlay => AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;
        public static bool IsInTask => InGame && !MeetingHud.Instance;
        public static bool IsMeeting => InGame && MeetingHud.Instance;
        public static bool IsVoting => IsMeeting && MeetingHud.Instance.state is MeetingHud.VoteStates.Voted or MeetingHud.VoteStates.NotVoted;
        public static bool IsCountDown => GameStartManager.InstanceExists && GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown;
        public static bool IsShip => ShipStatus.Instance != null;
        public static bool IsCanMove => PlayerControl.LocalPlayer?.CanMove is true;
        public static bool IsDead => PlayerControl.LocalPlayer?.Data?.IsDead is true;
        public static bool IsStandardMode => SwitchGameModePatch.HnSMode is false;
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




    public static PlayerData GetPlayerData(this PlayerControl pc) => PlayerData.GetPlayerDataById(pc.PlayerId) ?? null;
    public static bool IsAlive(this PlayerControl pc) => GameStates.IsLobby || pc?.GetPlayerData()?.IsDead == false;
    public static string GetDataName(this PlayerControl pc) => PlayerData.GetPlayerNameById(pc.PlayerId);
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
    public static bool IsModClient(this PlayerControl player) => Main.playerVersion.ContainsKey(player.PlayerId);

}


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
internal class DataFixedUpdate
{
    static bool Prefix(PlayerControl __instance)
    {
        if (!XtremeGameData.GameStates.IsInTask) return true;
        DisconnectSync(__instance);
        DeathSync(__instance);
        DeathReasonSync(__instance);
        return true;
    }
    static void DisconnectSync(PlayerControl pc)
    {
        var data = pc.GetPlayerData();
        var currectlyDisconnect = pc.Data.Disconnected && !data.IsDisconnected;
        var Task_NotAssgin = data.TotalTaskCount == 0 && !data.IsImpostor;
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