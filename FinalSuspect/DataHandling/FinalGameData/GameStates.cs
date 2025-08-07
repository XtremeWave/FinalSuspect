using System;
using AmongUs.GameOptions;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using InnerNet;

namespace FinalSuspect.DataHandling.FinalGameData;

public partial class FinalGameData
{
    public static class GameStates
    {
        private static bool InGame { get; set; }
        private static bool InMeeting { get; set; }

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
        public static bool IsInMeeting => IsInGame && MeetingHud.Instance && InMeeting;

        public static bool IsVoting => IsInMeeting &&
                                       MeetingHud.Instance.state is MeetingHud.VoteStates.Voted
                                           or MeetingHud.VoteStates.NotVoted;

        public static bool IsCountDown => GameStartManager.InstanceExists &&
                                          GameStartManager.Instance.startState ==
                                          GameStartManager.StartingStates.Countdown;

        public static bool IsShip => ShipStatus.Instance;
        public static bool IsCanMove => PlayerControl.LocalPlayer?.CanMove is true;
        public static bool IsDead => PlayerControl.LocalPlayer?.Data?.IsDead is true && !IsLobby;

        public static bool IsNormalGame =>
            GameOptionsManager.Instance.CurrentGameOptions.GameMode is GameModes.Normal or GameModes.NormalFools;

        public static bool IsHideNSeek =>
            GameOptionsManager.Instance.CurrentGameOptions.GameMode is GameModes.HideNSeek or GameModes.SeekFools;

        public static bool IsVanillaServer
        {
            get
            {
                if (!IsOnlineGame) return false;
                const string domain = "among.us";
                //Reactor.gg
                return ServerManager.Instance.CurrentRegion?.TryCast<StaticHttpRegionInfo>() is { } regionInfo &&
                       regionInfo.PingServer.EndsWith(domain, StringComparison.Ordinal) &&
                       regionInfo.Servers.All(serverInfo => serverInfo.Ip.EndsWith(domain, StringComparison.Ordinal));
            }
        }

        public static void UpdateGameState_IsInGame(bool inGame)
        {
            InGame = inGame;
        }

        public static void UpdateGameState_IsInMeeting(bool inMeeting)
        {
            InMeeting = inMeeting;
        }

        public static bool MapIsActive(MapNames name)
        {
            return (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == name;
        }
    }
}