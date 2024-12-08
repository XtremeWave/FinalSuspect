using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using System;
using System.Linq;
using System.Collections.Generic;
using static FinalSuspect.Translator;
using Il2CppSystem.Runtime.Remoting.Messaging;

namespace FinalSuspect;
internal class FAC
{
    public static int MeetingTimes = 0;
    public static int DeNum = 0;
    private static List<byte> LobbyDeadBodies = [];
    public static void Init()
    {
        DeNum = new();
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
                    ErrorText.Instance.SBDetected ? 
                    ErrorCode.SBDetected : ErrorCode.CheatDetected);
            else
                ErrorText.Instance.Clear();
        }
    }
    public static bool ReceiveRpc(PlayerControl pc, byte callId, MessageReader reader, out bool notify)
    {
        notify = true;
        if (pc == null || reader == null || pc.AmOwner) return false;
        try
        {
            MessageReader sr = MessageReader.Get(reader);
            var rpc = (RpcCalls)callId;

            if (CheckForInvalidRpc(pc, callId))
            {
                return true;
            }

            switch (rpc)
            {
                case RpcCalls.SetName:
                case RpcCalls.CheckName:
                    if (CheckForSetName(pc))
                        return true;
                    break;
                case RpcCalls.SetColor:
                case RpcCalls.CheckColor:
                    var color = sr.ReadByte();
                    if (pc.Data.DefaultOutfit.ColorId != -1 &&
                        (color < 0 
                        || color > 18))
                    {
                        return true;
                    }
                    break;
                case RpcCalls.EnterVent:
                case RpcCalls.ExitVent:
                case RpcCalls.BootFromVent:
                    if (pc.IsImpostor() || pc.GetRoleType() == RoleTypes.Engineer)
                        break;
                    return true;

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
                        if (!AmongUsClient.Instance.AmHost) 
                            RPC.NotificationPop(GetString("Warning.RoomBroken"));
                        notify = false;
                        return true;
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
                        return true;
                }
            if (XtremeGameData.GameStates.IsMeeting)
                switch (rpc)
                {
                    case RpcCalls.CheckMurder:
                    case RpcCalls.MurderPlayer:
                    case RpcCalls.SetColor:
                    case RpcCalls.CheckColor:
                    case RpcCalls.SetName:
                    case RpcCalls.CheckName:
                        return true;
                }
        }
        catch (Exception e)
        {
            Logger.Exception(e, "FAC");
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
    static bool CheckForInvalidRpc(PlayerControl player, byte callId)
    {
        if (player.PlayerId != 0 && !Enum.IsDefined(typeof(RpcCalls), callId) && !XtremeGameData.GameStates.OtherModHost)
        {
            Logger.Warn($"{player?.Data?.PlayerName}:{callId}({RPC.GetRpcName(callId)}) 已取消，因为它是由主机以外的其他人发送的。", "CustomRPC");
            if (AmongUsClient.Instance.AmHost)
            {
                if (!ReceiveInvalidRpc(player, callId)) return true;
                Utils.KickPlayer(player.GetClientId(), false, "InvalidRPC");
                Logger.Warn($"收到来自 {player?.Data?.PlayerName} 的不受信用的RPC，因此将其踢出。", "Kick");
                RPC.NotificationPop(string.Format(GetString("Warning.InvalidRpc"), player?.Data?.PlayerName));
                return true;

            }
            else
            {
                Logger.Warn($"收到来自 {player?.Data?.PlayerName} 的不受信用的RPC", "Kick?");
                RPC.NotificationPop(string.Format(GetString("Warning.InvalidRpc_NotHost"), player?.Data?.PlayerName, callId));
                if (!player.cosmetics.nameText.text.Contains("<color=#ff0000>"))
                    player.cosmetics.nameText.text = "<color=#ff0000>" + player.cosmetics.nameText.text + "</color>";
                return true;
            }
        }
        return false;
    }
    static bool CheckForSetName(PlayerControl player)
    {
        if (OnPlayerJoinedPatch.SetNameNum[player.PlayerId] > 3)
        {
            Logger.Warn($"{player?.Data?.PlayerName}多次设置名称", "CustomRPC");
            if (AmongUsClient.Instance.AmHost)
            {
                Utils.KickPlayer(player.GetClientId(), true, "SetName");
                Logger.Warn($"收到来自 {player?.Data?.PlayerName} 的多次设置名称，因此将其踢出。", "Kick");
                RPC.NotificationPop(string.Format(GetString("Warning.SetName"), player?.Data?.PlayerName));
                WarnHost();
                return true;

            }
            else if (!XtremeGameData.GameStates.OtherModHost)
            {
                Logger.Warn($"收到来自 {player?.Data?.PlayerName}({player?.FriendCode}) 的多次设置名称", "Kick?");
                RPC.NotificationPop(string.Format(GetString("Warning.SetName_NotHost"), player?.Data?.PlayerName));
                if (!player.cosmetics.nameText.text.Contains("<color=#ff0000>"))
                    player.cosmetics.nameText.text = "<color=#ff0000>" + player.cosmetics.nameText.text + "</color>";
                return true;
            }
        }
        return false;
    }

    public static bool ReceiveInvalidRpc(PlayerControl pc, byte callId)
    {

        switch (callId)
        {
            case unchecked((byte)42069):
                //Report(pc, "AUM");
                HandleCheat(pc, GetString("FAC.CheatDetected.FAC"));
                return true;
            case 101:
                //Report(pc, "AUM");
                HandleCheat(pc, GetString("FAC.CheatDetected.FAC"));
                return true;
            case unchecked((byte)520)://YuMenu
                //Report(pc, "YM");
                HandleCheat(pc, GetString("FAC.CheatDetected.FAC"));
                return true;
            case 168:
                //Report(pc, "SM");
                HandleCheat(pc, GetString("FAC.CheatDetected.FAC"));
                return true;
            case unchecked((byte)420):
                //Report(pc, "SM");
                HandleCheat(pc, GetString("FAC.CheatDetected.FAC"));
                return true;
            case 119: 
                //Report(pc, "KN");
                HandleCheat(pc, GetString("FAC.CheatDetected.FAC"));
                return true;
            case unchecked((byte)250): 
                //Report(pc, "KN");
                HandleCheat(pc, GetString("FAC.CheatDetected.FAC"));
                return true;
        }
        return true;
    }
    public static void HandleCheat(PlayerControl pc, string text)
    {
        Utils.KickPlayer(pc.GetClientId(), true, "CheatDetected");
        RPC.NotificationPop(pc.GetDataName() + text);

    }
}