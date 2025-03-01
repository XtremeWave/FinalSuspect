using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using FinalSuspect.Patches.Game_Vanilla;
using Hazel;
using InnerNet;
using Unity.Services.Core.Internal;
using static FinalSuspect.DataHandling.FinalAntiCheat.FAC;


namespace FinalSuspect.DataHandling;

public static class FinalAntiCheat
{
    public class PlayerCheatData
    {
        public bool IsSuspectCheater { get; private set; }
        public int SetNameTimes { get; private set; }

        private PlayerControl Player { get; }
        public ClientData ClientData { get; }
        public string FriendCode => ClientData.FriendCode;
        public string Puid => ClientData.GetHashedPuid();
        public int SendQuickMessageCountPerSecond { get; private set; }

        public PlayerCheatData(PlayerControl player)
        {
            IsSuspectCheater = false;
            SetNameTimes = 
            SendQuickMessageCountPerSecond = 0;
            _lastKillTime = _lastSendTime = -1;
            Player = player;
            ClientData = Player.GetClient();
        }
        private long _lastSendTime; 
        private const long ResetInterval = 3;

        private long _lastKillTime;
        private const long AdjustDelay = 1;
        public bool HandleSetName()
        {
            SetNameTimes++;
            if (SetNameTimes > 3)
            {
                var name = Player.GetDataName();
                XtremeLogger.Warn($"{name}({FriendCode})({Puid})多次设置名称", "FAC");
                if (AmongUsClient.Instance.AmHost)
                {
                    NotificationPopperPatch.NotificationPop(
                        string.Format(GetString("Warning.SetName"),
                            name));
                    WarnHost();
                    return true;
                }
                if (!XtremeGameData.GameStates.OtherModHost)
                {
                    NotificationPopperPatch.NotificationPop(
                        string.Format(GetString("Warning.SetName_NotHost"),
                            name));
                    return true;
                }
            }

            return false;
        }
        public bool HandleSendQuickChat()
        {
            if (_lastSendTime != -1 && _lastSendTime + ResetInterval > Utils.GetTimeStamp())
            {
                SendQuickMessageCountPerSecond++;
                if (SendQuickMessageCountPerSecond > 1)
                {
                    var name = Player.GetDataName();
                    XtremeLogger.Warn($"{name}({FriendCode})({Puid})一秒内多次发送快捷消息", "FAC");
                    if (AmongUsClient.Instance.AmHost)
                    {
                        NotificationPopperPatch.NotificationPop(
                            string.Format(GetString("Warning.SendQuickChat"),
                                name));
                        WarnHost();
                        return true;
                    }

                    if (!XtremeGameData.GameStates.OtherModHost)
                    {
                        NotificationPopperPatch.NotificationPop(
                            string.Format(GetString("Warning.SendQuickChat_NotHost"),
                                name));
                        return true;
                    }
                }
            }
            _lastSendTime = Utils.GetTimeStamp();
            return false;
        }
        public bool HandleMurderPlayer(PlayerControl target)
        {
            var KillOnePlayerForManyTimes = _lastKillTime != -1 && _lastKillTime + AdjustDelay < Utils.GetTimeStamp() && !target.IsAlive();
            _lastKillTime = Utils.GetTimeStamp();
            return KillOnePlayerForManyTimes || !Player.IsImpostor();
        }
        public void HandleBan()
        {
            if (ClientData.IsFACPlayer() || ClientData.IsBannedPlayer())
                MarkAsCheater();
        }
        public void HandleLobbyPosition()
        {
            if (XtremeGameData.GameStates.IsLobby)
            {


                var posXOutOfRange = Player.GetTruePosition().x > 3.5f || Player.GetTruePosition().x < -3.5f;
                var posYOutOfRange = Player.GetTruePosition().y > 4f || Player.GetTruePosition().y < -1f;
                if (posXOutOfRange || posYOutOfRange)
                    MarkAsCheater();
            }
            else
            {
                /*List<PlainShipRoom> rooms = [];
                foreach (var room in ShipStatus.Instance.FastRooms)
                {
                    rooms.Add(room.Value);
                }
                rooms.AddRange(ShipStatus.Instance.AllRooms);

                if (rooms.Any(room => Player.Collider.IsTouching(room.roomArea)) 
                    || rooms.Any(room => room.roomArea.OverlapPoint(Player.GetTruePosition())) 
                    || rooms.Any(room => room.roomArea.IsTouching(Player.Collider)) 
                    || !Player.IsAlive()) return;
                MarkAsCheater();*/
            }
        }
        public void HandleSuspectCheater()
        {
            if (Main.DisableFAC.Value || !IsSuspectCheater || _lastHandleCheater != -1 && _lastHandleCheater + 1 >= Utils.GetTimeStamp()) return;
            _lastHandleCheater = Utils.GetTimeStamp();
            NotificationPopperPatch.NotificationPop(string.Format(GetString("Warning.SetName_NotHost"),
                Player.GetDataName()));
            if (!AmongUsClient.Instance.AmHost)return;
            Utils.KickPlayer(Player.PlayerId, false, "Suspect Cheater");
            
        }
        public void MarkAsCheater() => IsSuspectCheater = true;
    }
    internal class FAC
    {
        public static int MeetingTimes = 0;
        public static int DeNum;
        public static long _lastHandleCheater = -1;
        private static List<byte> LobbyDeadBodies = [];

        public static void Init()
        {
            DeNum = 0;
            LobbyDeadBodies = [];
        }

        public static void WarnHost(int denum = 1)
        {
            DeNum += denum;
            if (ErrorText.Instance != null)
            {
                ErrorText.Instance.CheatDetected = DeNum > 3;
                ErrorText.Instance.SBDetected = DeNum > 10;
                if (ErrorText.Instance.CheatDetected)
                    ErrorText.Instance.AddError(
                        ErrorText.Instance.SBDetected ? ErrorCode.SBDetected : ErrorCode.CheatDetected);
                else
                    ErrorText.Instance.Clear();
            }
        }

        public static bool ReceiveRpc(PlayerControl pc, byte callId, MessageReader reader, out bool notify,
            out string reason, out bool ban)
        {
            notify = true;
            reason = "Hacking";
            ban = false;
            if (Main.DisableFAC.Value || pc == null || reader == null || pc.AmOwner) return false;
            try
            {
                var sr = MessageReader.Get(reader);
                var rpc = (RpcCalls)callId;

                if (CheckForInvalidRpc(pc, callId))
                {
                    notify = false;
                    return true;
                }


                switch (rpc)
                {
                    case RpcCalls.SetName:
                    case RpcCalls.CheckName:
                        if (pc.GetCheatData().HandleSetName())
                        {
                            ban = true;
                            notify = false;
                            return true;
                        }
                        break;
                    case RpcCalls.EnterVent:
                    case RpcCalls.ExitVent:
                    case RpcCalls.BootFromVent:
                        if (pc.IsImpostor() || pc.GetRoleType() == RoleTypes.Engineer)
                            break;
                        return true;
                    case RpcCalls.SendChat:
                        var text = sr.ReadString();
                        if (text.Length > 100)
                            return true;
                        break;

                }

                if (XtremeGameData.GameStates.IsLobby)
                    switch (rpc)
                    {
                        case RpcCalls.CheckMurder:
                        case RpcCalls.MurderPlayer:
                        case RpcCalls.Exiled:
                        case RpcCalls.EnterVent:
                        case RpcCalls.ExitVent:
                        case RpcCalls.BootFromVent:
                        case RpcCalls.SendChatNote:
                        case RpcCalls.StartMeeting:
                        case RpcCalls.ReportDeadBody:
                        case RpcCalls.CheckProtect:
                        case RpcCalls.ProtectPlayer: 
                        case RpcCalls.AddVote:
                        case RpcCalls.CastVote:
                        case RpcCalls.ClearVote:
                        case RpcCalls.VotingComplete: 
                        case RpcCalls.ClimbLadder:
                        case RpcCalls.UpdateSystem:
                            if (AmongUsClient.Instance.AmHost) return true;
                            NotificationPopperPatch.NotificationPop(GetString("Warning.RoomBroken"));
                            notify = false;
                            return true;
                        case RpcCalls.SendQuickChat:
                            if (pc.GetCheatData().HandleSendQuickChat())
                            {
                                ban = true;
                                notify = false;
                                return true;
                            }
                            break;
                    }

                if (XtremeGameData.GameStates.IsInTask)
                    switch (rpc)
                    {
                        case RpcCalls.Exiled:
                        case RpcCalls.SendChatNote:
                        case RpcCalls.SetColor:
                        case RpcCalls.CheckColor:
                        case RpcCalls.SetName:
                        case RpcCalls.CheckName:
                        case RpcCalls.SetLevel:
                        case RpcCalls.SetHat:
                            return true;
                        case RpcCalls.SendChat:
                        case RpcCalls.SendQuickChat:
                            if (pc.IsAlive()) return true;
                            break;
                        case RpcCalls.CheckMurder:
                        case RpcCalls.MurderPlayer:
                            var target = sr.ReadNetObject<PlayerControl>();
                            if (pc.GetCheatData().HandleMurderPlayer(target))
                                return true;
                            break;
                        case RpcCalls.ReportDeadBody:
                            var deadbody = Utils.GetPlayerById(sr.ReadByte());
                            if (deadbody == null || !deadbody.IsAlive())
                                break;
                            return true;
                    }

                if (XtremeGameData.GameStates.IsMeeting)
                    switch (rpc)
                    {
                        case RpcCalls.CheckMurder:
                        case RpcCalls.MurderPlayer:
                            if (pc.IsImpostor())
                                break;
                            return true;
                        case RpcCalls.SetColor:
                        case RpcCalls.CheckColor:
                        case RpcCalls.SetName:
                        case RpcCalls.CheckName:
                            return true;
                        case RpcCalls.SendQuickChat:
                            if (pc.GetCheatData().HandleSendQuickChat())
                            {
                                ban = true;
                                notify = false;
                                return true;
                            }
                            break;
                    }
            }
            catch (Exception e)
            {
                XtremeLogger.Exception(e, "FAC");
                throw;
            }

            WarnHost(-1);
            return false;
        }

        //public static void Report(PlayerControl pc, string reason)
        //{
        //    //string msg = $"{pc.GetClientId()}|{pc.FriendCode}|{pc.Data.PlayerName}|{reason}";
        //    //Cloud.SendData(msg);
        //    //Logger.Warn($"FAC报告：{pc.GetRealName()}: {reason}", "FAC Cloud");
        //}
        private static bool CheckForInvalidRpc(PlayerControl player, byte callId)
        {
            if (player.PlayerId != 0 && !Enum.IsDefined(typeof(RpcCalls), callId) &&
                !XtremeGameData.GameStates.OtherModHost)
            {
                XtremeLogger.Warn(
                    $"{player?.Data?.PlayerName}:{callId}({RPC.GetRpcName(callId)}) 已取消，因为它是由主机以外的其他人发送的。",
                    "FAC");
                if (ReceiveInvalidRpc(player, callId)) return true;
                if (AmongUsClient.Instance.AmHost)
                {
                   
                    XtremeLogger.Warn($"收到来自 {player?.Data?.PlayerName} 的不受信用的RPC，因此将其踢出。", "Kick");
                    NotificationPopperPatch.NotificationPop(string.Format(GetString("Warning.InvalidRpc"),
                        player?.Data?.PlayerName, callId));
                    return true;

                }

                XtremeLogger.Warn($"收到来自 {player?.Data?.PlayerName} 的不受信用的RPC", "Kick?");
                NotificationPopperPatch.NotificationPop(string.Format(GetString("Warning.InvalidRpc_NotHost"),
                    player?.Data?.PlayerName, callId));
                return true;
            }

            return false;
        }

        private static bool ReceiveInvalidRpc(PlayerControl pc, byte callId)
        {
            switch (callId)
            {
                case unchecked((byte)42069):
                case 101:
                    //Report(pc, "AUM");
                    HandleCheat(pc, GetString("FAC.CheatDetected.FAC"));
                    return true;
                case unchecked((byte)520): //YuMenu
                    //Report(pc, "YM");
                    HandleCheat(pc, GetString("FAC.CheatDetected.FAC"));
                    return true;
                case 168:
                case unchecked((byte)420):
                    //Report(pc, "SM");
                    HandleCheat(pc, GetString("FAC.CheatDetected.FAC"));
                    return true;
                case 119:
                case unchecked(250):
                    //Report(pc, "KN");
                    HandleCheat(pc, GetString("FAC.CheatDetected.FAC"));
                    return true;
            }

            return false;
        }

        public static void HandleCheat(PlayerControl pc, string text)
        {
            NotificationPopperPatch.NotificationPop(pc.GetDataName() + text);
        }
    }

    public static void MarkAsCheater(this PlayerControl pc)
    {
        pc.GetXtremeData().CheatData.MarkAsCheater();
    }
    public static PlayerCheatData GetCheatDataById(byte id)
    {
        try
        {
            return XtremePlayerData.GetXtremeDataById(id)?.CheatData;
        }
        catch
        {
            return null;
        }
    }
}