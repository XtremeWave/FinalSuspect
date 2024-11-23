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
            if (AmongUsClient.Instance.AmHost)
            ErrorText.Instance.CheatDetected = DeNum > 3;
            ErrorText.Instance.SBDetected = DeNum > 10;
            if (ErrorText.Instance.CheatDetected)
                ErrorText.Instance.AddError(ErrorText.Instance.SBDetected ? ErrorCode.SBDetected : ErrorCode.CheatDetected);
            else
                ErrorText.Instance.Clear();
        }
    }
    public static bool ReceiveRpc(PlayerControl pc, byte callId, MessageReader reader)
    {
        if (pc == null || reader == null || pc.AmOwner || !AmongUsClient.Instance.AmHost) return false;
        //if (pc.GetClient()?.PlatformData?.Platform is Platforms.Android or Platforms.IPhone or Platforms.Switch or Platforms.Playstation or Platforms.Xbox or Platforms.StandaloneMac) return false;
        try
        {
            MessageReader sr = MessageReader.Get(reader);
            var rpc = (RpcCalls)callId;
            switch (rpc)
            {
                
                case RpcCalls.SetName:
                case RpcCalls.CheckName:
                    if (!XtremeGameData.GameStates.IsLobby)
                    {
                        WarnHost();
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法修改名字，已驳回", "FAC");
                        return true;
                    }
                    break;
                case RpcCalls.SendChatNote:
                    if (XtremeGameData.GameStates.IsLobby || XtremeGameData.GameStates.IsInTask)
                    {
                        var murdered = sr.ReadNetObject<PlayerControl>();

                        Report(pc, "非法发送投票标签");
                        if (murdered != null && !LobbyDeadBodies.Contains(murdered.PlayerId))
                        {
                            LobbyDeadBodies.Add(murdered.PlayerId);
                        }
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法发送投票标签，已驳回", "FAC");
                        return true;
                    }
                    break;
                case RpcCalls.SendChat:
                    var text = sr.ReadString();
                    if (text.StartsWith("/")) return false;
                    if (
                        text.Contains("░") ||
                        text.Contains("▄") ||
                        text.Contains("█") ||
                        text.Contains("▌") ||
                        text.Contains("▒") ||
                        text.Contains("习近平")
                        )
                    {
                        Report(pc, "非法消息");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】发送非法消息，已驳回", "FAC");
                        return true;
                    }
                    break;
                case RpcCalls.StartMeeting:
                    MeetingTimes++;
                    if ((XtremeGameData.GameStates.IsMeeting && MeetingTimes > 3) || XtremeGameData.GameStates.IsLobby)
                    {
                        WarnHost();
                        Report(pc, "非法召集会议");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法召集会议：【null】，已驳回", "FAC");
                        return true;
                    }
                    break;
                case RpcCalls.ReportDeadBody:
                    var bodyid = sr.ReadByte();
                    if (XtremeGameData.GameStates.IsLobby || XtremeGameData.GameStates.IsMeeting)
                    {
                        WarnHost();
                        Report(pc, "非法报告尸体");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法报告尸体，已驳回", "FAC");
                        return true;
                    }
                    break;
                //case RpcCalls.SetColor:
                //case RpcCalls.CheckColor:
                //    var color = sr.ReadByte();
                //    if (pc.Data.DefaultOutfit.ColorId != -1 &&
                //        (Main.AllPlayerControls.Where(x => x.Data.DefaultOutfit.ColorId == color).Count() >= 5
                //        || XtremeGameData.GameStates.IsInGame || color < 0 || color > 18))
                //    {
                //        WarnHost();
                //        Report(pc, "非法设置颜色");
                //        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法设置颜色，已驳回", "FAC");
                //        return true;
                //    }
                //    break;
                case RpcCalls.MurderPlayer:
                    if (XtremeGameData.GameStates.IsLobby || XtremeGameData.GameStates.IsMeeting)
                    {
                        var murdered = sr.ReadNetObject<PlayerControl>();

                        Report(pc, "非法击杀");
                        if (murdered != null && !LobbyDeadBodies.Contains(murdered.PlayerId))
                        {
                            LobbyDeadBodies.Add(murdered.PlayerId);
                        }
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法击杀，已驳回", "FAC");
                        return true;
                    }
                    break;
                case RpcCalls.CheckMurder:
                    if (XtremeGameData.GameStates.IsLobby || XtremeGameData.GameStates.IsMeeting)
                    {
                        WarnHost();
                        Report(pc, "非法检查击杀");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法检查击杀，已驳回", "FAC");
                        return true;
                    }
                    break;
                case RpcCalls.BootFromVent:
                    if (XtremeGameData.GameStates.IsLobby)
                    {
                        WarnHost();
                        Report(pc, "非法在大厅炸管");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法在大厅炸管，已驳回", "FAC");
                        return true;
                    }
                    break;


            }

            switch (callId)
            {
                case 7:
                case 8:
                    if (XtremeGameData.GameStates.IsInGame)
                    {
                        WarnHost();
                        Report(pc, "非法设置颜色");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法设置颜色，已驳回", "FAC");
                        return true;
                    }
                    break;
                case 11:
                    MeetingTimes++;
                    if ((XtremeGameData.GameStates.IsMeeting && MeetingTimes > 3) || XtremeGameData.GameStates.IsLobby)
                    {
                        WarnHost();
                        Report(pc, "非法召集会议");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法召集会议：【null】，已驳回", "FAC");
                        return true;
                    }
                    break;
                case 5:
                    string name = sr.ReadString();
                    if (XtremeGameData.GameStates.IsInGame)
                    {
                        WarnHost();
                        Report(pc, "非法设置游戏名称");
                        Logger.Fatal($"非法修改玩家【{pc.GetClientId()}:{pc.GetRealName()}】的游戏名称，已驳回", "FAC");
                        return true;
                    }
                    break;
                case 47:
                    if (XtremeGameData.GameStates.IsLobby)
                    {
                        WarnHost();
                        Report(pc, "非法击杀");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法击杀，已驳回", "FAC");
                        return true;
                    }
                    break;
                case 12:
                    if (XtremeGameData.GameStates.IsLobby)
                    {
                        WarnHost();
                        Report(pc, "非法击杀");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法击杀，已驳回", "FAC");
                        return true;
                    }
                    break;
                case 34:
                    if (XtremeGameData.GameStates.IsLobby)
                    {
                        WarnHost();
                        Report(pc, "非法在大厅炸管");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法在大厅炸管，已驳回", "FAC");
                        return true;
                    }
                    break;
                case 41:
                    if (XtremeGameData.GameStates.IsInGame)
                    {
                        WarnHost();
                        Report(pc, "非法设置宠物");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法设置宠物，已驳回", "FAC");
                        return true;
                    }
                    break;
                case 40:
                    if (XtremeGameData.GameStates.IsInGame)
                    {
                        WarnHost();
                        Report(pc, "非法设置皮肤");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法设置皮肤，已驳回", "FAC");
                        return true;
                    }
                    break;
                case 42:
                    if (XtremeGameData.GameStates.IsInGame)
                    {
                        WarnHost();
                        Report(pc, "非法设置面部装扮");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法设置面部装扮，已驳回", "FAC");
                        return true;
                    }
                    break;
                case 39:
                    if (XtremeGameData.GameStates.IsInGame)
                    {
                        WarnHost();
                        Report(pc, "非法设置帽子");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法设置帽子，已驳回", "FAC");
                        return true;
                    }
                    break;
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
    public static void Report(PlayerControl pc, string reason)
    {
        //string msg = $"{pc.GetClientId()}|{pc.FriendCode}|{pc.Data.PlayerName}|{reason}";
        //Cloud.SendData(msg);
        //Logger.Warn($"FAC报告：{pc.GetRealName()}: {reason}", "FAC Cloud");
    }
    public static bool ReceiveInvalidRpc(PlayerControl pc, byte callId)
    {

        switch (callId)
        {
            case unchecked((byte)42069):
                Report(pc, "AUM");
                HandleCheat(pc, GetString("FAC.CheatDetected.FAC"));
                return true;
            case 101:
                Report(pc, "AUM");
                HandleCheat(pc, GetString("FAC.CheatDetected.FAC"));
                return true;
            //Slok你个歌姬
            case unchecked((byte)520)://YuMenu
                Report(pc, "YM");
                HandleCheat(pc, GetString("FAC.CheatDetected.FAC"));
                return true;
            case 168:
                Report(pc, "SM");
                HandleCheat(pc, GetString("FAC.CheatDetected.FAC"));
                return true;
            case unchecked((byte)420):
                Report(pc, "SM");
                HandleCheat(pc, GetString("FAC.CheatDetected.FAC"));
                return true;
            case 119: 
                Report(pc, "KN");
                HandleCheat(pc, GetString("FAC.CheatDetected.FAC"));
                return true;
            case unchecked((byte)250): 
                Report(pc, "KN");
                HandleCheat(pc, GetString("FAC.CheatDetected.FAC"));
                return true;
        }
        return true;
    }
    public static void HandleCheat(PlayerControl pc, string text)
    {
        Utils.KickPlayer(pc.GetClientId(), true, "CheatDetected");
        //switch (Options.CheatResponses.GetInt())
        //{
        //    case 0:
        //        Utils.KickPlayer(pc.GetClientId(), true, "CheatDetected");
        //        string msg0 = string.Format(GetString("Message.KickedByFAC"), pc?.Data?.PlayerName, text);
        //        Logger.Warn(msg0, "FAC");
        //        RPC.NotificationPop(msg0);
        //        break;
        //    case 1:
        //        Utils.KickPlayer(pc.GetClientId(), false, "CheatDetected");
        //        string msg1 = string.Format(GetString("Message.BanedByFAC"), pc?.Data?.PlayerName, text);
        //        Logger.Warn(msg1, "FAC");
        //        RPC.NotificationPop(msg1);
        //        break;
        //    case 2:
        //        Utils.SendMessage(string.Format(GetString("Message.NoticeByFAC"), pc?.Data?.PlayerName, text), PlayerControl.LocalPlayer.PlayerId, Utils.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), GetString("MessageFromFAC")));
        //        break;
        //    case 3:
        //        foreach (var apc in Main.AllPlayerControls.Where(x => x.PlayerId != pc?.Data?.PlayerId))
        //            Utils.SendMessage(string.Format(GetString("Message.NoticeByFAC"), pc?.Data?.PlayerName, text), pc.PlayerId, Utils.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), GetString("MessageFromFAC")));
        //        break;
        //}
    }
}