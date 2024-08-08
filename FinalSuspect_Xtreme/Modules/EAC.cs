using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using System;
using System.Linq;
using System.Collections.Generic;
using static FinalSuspect_Xtreme.Translator;

namespace FinalSuspect_Xtreme;
internal class EAC
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
        if (pc == null || reader == null || pc.AmOwner) return false;
        //if (pc.GetClient()?.PlatformData?.Platform is Platforms.Android or Platforms.IPhone or Platforms.Switch or Platforms.Playstation or Platforms.Xbox or Platforms.StandaloneMac) return false;
        try
        {
            MessageReader sr = MessageReader.Get(reader);
            var rpc = (RpcCalls)callId;
            switch (rpc)
            {
                //case RpcCalls.SetName:
                //    string name = sr.ReadString();
                //    if (sr.BytesRemaining > 0 && sr.ReadBoolean()) return false;
                //    if (
                //        ((name.Contains("<size") || name.Contains("size>")) && name.Contains("?") && !name.Contains("color")) ||
                //        name.Length > 160 ||
                //        name.Count(f => f.Equals("\"\\n\"")) > 3 ||
                //        name.Count(f => f.Equals("\n")) > 3 ||
                //        name.Count(f => f.Equals("\r")) > 3 ||
                //        name.Contains("░") ||
                //        name.Contains("▄") ||
                //        name.Contains("█") ||
                //        name.Contains("▌") ||
                //        name.Contains("▒") ||
                //        name.Contains("习近平")
                //        )
                //    {
                //        WarnHost();
                //        Report(pc, "非法设置游戏名称");
                //        Logger.Fatal($"非法修改玩家【{pc.GetClientId()}:{pc.GetRealName()}】的游戏名称，已驳回", "EAC");
                //        return true;
                //    }
                //    break;
                case RpcCalls.CheckName:
                    if (!GameStates.IsLobby)
                    {
                        WarnHost();
                        Report(pc, "非法修改名字");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法修改名字，已驳回", "EAC");
                        return true;
                    }
                    break;
                //case RpcCalls.SetRole:
                //    var role = (RoleTypes)sr.ReadUInt16();
                //    if (GameStates.IsLobby && (role is RoleTypes.CrewmateGhost or RoleTypes.ImpostorGhost))
                //    {
                //        WarnHost();
                //        Report(pc, "非法设置状态为幽灵");
                //        Logger.Fatal($"非法设置玩家【{pc.GetClientId()}:{pc.GetRealName()}】的状态为幽灵，已驳回", "EAC");
                //        return true;
                //    }
                //    break;
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
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】发送非法消息，已驳回", "EAC");
                        return true;
                    }
                    break;
                case RpcCalls.StartMeeting:
                    MeetingTimes++;
                    if ((GameStates.IsMeeting && MeetingTimes > 3) || GameStates.IsLobby || GameStates.IsInTask)
                    {
                        WarnHost();
                        Report(pc, "非法召集会议");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法召集会议：【null】，已驳回", "EAC");
                        return true;
                    }
                    break;
                case RpcCalls.ReportDeadBody:
                    var bodyid = sr.ReadByte();
                    if (!GameStates.IsInGame || GameStates.IsInTask)
                    {
                        WarnHost();
                        Report(pc, "非法报告尸体");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法报告尸体，已驳回", "EAC");
                        return true;
                    }
                    else
                    {
                        Report(pc, "尝试举报可能被非法击杀的尸体");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】尝试举报可能被非法击杀的尸体，已驳回", "EAC");
                    }
                    break;
                case RpcCalls.SetColor:
                case RpcCalls.CheckColor:
                    var color = sr.ReadByte();
                    if (pc.Data.DefaultOutfit.ColorId != -1 &&
                        (Main.AllPlayerControls.Where(x => x.Data.DefaultOutfit.ColorId == color).Count() >= 5
                        || !GameStates.IsLobby || color < 0 || color > 18))
                    {
                        WarnHost();
                        Report(pc, "非法设置颜色");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法设置颜色，已驳回", "EAC");
                        return true;
                    }
                    break;
                case RpcCalls.MurderPlayer:
                    var murdered = sr.ReadNetObject<PlayerControl>();
                    if (GameStates.IsLobby || GameStates.IsInTask)
                    {
                        Report(pc, "大厅直接击杀");
                        if (murdered != null && !LobbyDeadBodies.Contains(murdered.PlayerId))
                        {
                            LobbyDeadBodies.Add(murdered.PlayerId);
                        }
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】大厅直接击杀，已驳回", "EAC");
                        return true;
                    }
                    Report(pc, "直接击杀");
                    Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】直接击杀，已驳回", "EAC");
                    return true;
                case RpcCalls.CheckMurder:
                    if (GameStates.IsLobby || GameStates.IsInTask)
                    {
                        WarnHost();
                        Report(pc, "CheckMurder在大厅");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法检查击杀，已驳回", "EAC");
                        return true;
                    }
                    break;
                case RpcCalls.BootFromVent:
                    if (GameStates.IsLobby)
                    {
                        WarnHost();
                        Report(pc, "非法在大厅炸管");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法在大厅炸管，已驳回", "EAC");
                        return true;
                    }
                    break;
                    //case RpcCalls.ExtendLobbyTimer:
                    //{
                    //    if (GameStates.IsLobby)
                    //    {
                    //        WarnHost();
                    //        Report(pc, "非法延长大厅计时器");
                    //        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法延长大厅计时器，已驳回", "EAC");
                    //        return true;
                    //    }
                    //    break;

            }

            switch (callId)
            {
                case 101:
                    var AUMChat = sr.ReadString();
                    WarnHost();
                    Report(pc, "AUM");
                    HandleCheat(pc, GetString("EAC.CheatDetected.EAC"));
                    return true;
                case 7:
                case 8:
                    if (!GameStates.IsLobby)
                    {
                        WarnHost();
                        Report(pc, "非法设置颜色");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法设置颜色，已驳回", "EAC");
                        return true;
                    }
                    break;
                case 11:
                    MeetingTimes++;
                    if ((GameStates.IsMeeting && MeetingTimes > 3) || GameStates.IsLobby)
                    {
                        WarnHost();
                        Report(pc, "非法召集会议");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法召集会议：【null】，已驳回", "EAC");
                        return true;
                    }
                    break;
                case 5:
                    string name = sr.ReadString();
                    if (GameStates.IsInGame)
                    {
                        WarnHost();
                        Report(pc, "非法设置游戏名称");
                        Logger.Fatal($"非法修改玩家【{pc.GetClientId()}:{pc.GetRealName()}】的游戏名称，已驳回", "EAC");
                        return true;
                    }
                    break;
                case 47:
                    if (GameStates.IsLobby)
                    {
                        WarnHost();
                        Report(pc, "非法击杀");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法击杀，已驳回", "EAC");
                        return true;
                    }
                    break;
                case 12:
                    if (GameStates.IsLobby)
                    {
                        WarnHost();
                        Report(pc, "非法击杀");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法击杀，已驳回", "EAC");
                        return true;
                    }
                    break;
                case 34:
                    if (GameStates.IsLobby)
                    {
                        WarnHost();
                        Report(pc, "非法在大厅炸管");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法在大厅炸管，已驳回", "EAC");
                        return true;
                    }
                    break;
                case 41:
                    if (GameStates.IsInGame)
                    {
                        WarnHost();
                        Report(pc, "非法设置宠物");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法设置宠物，已驳回", "EAC");
                        return true;
                    }
                    break;
                case 40:
                    if (GameStates.IsInGame)
                    {
                        WarnHost();
                        Report(pc, "非法设置皮肤");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法设置皮肤，已驳回", "EAC");
                        return true;
                    }
                    break;
                case 42:
                    if (GameStates.IsInGame)
                    {
                        WarnHost();
                        Report(pc, "非法设置面部装扮");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法设置面部装扮，已驳回", "EAC");
                        return true;
                    }
                    break;
                case 39:
                    if (GameStates.IsInGame)
                    {
                        WarnHost();
                        Report(pc, "非法设置帽子");
                        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法设置帽子，已驳回", "EAC");
                        return true;
                    }
                    break;
                    //case 43:
                    //    if (sr.BytesRemaining > 0 && sr.ReadBoolean()) return false;
                    //    if (GameStates.IsInGame)
                    //    {
                    //        WarnHost();
                    //        Report(pc, "非法设置游戏名称");
                    //        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法设置名称，已驳回", "EAC");
                    //        return true;
                    //    }
                    //    break;
                    //case 61:
                    //    if (GameStates.IsLobby)
                    //    {
                    //        WarnHost();
                    //        Report(pc, "非法延长大厅计时器");
                    //        Logger.Fatal($"玩家【{pc.GetClientId()}:{pc.GetRealName()}】非法延长大厅计时器，已驳回", "EAC");
                    //        return true;
                    //    }
                    //    break;
            }
        }
        catch (Exception e)
        {
            Logger.Exception(e, "EAC");
            throw;
        }
        WarnHost(-1);
        return false;
    }
    public static void Report(PlayerControl pc, string reason)
    {
        string msg = $"{pc.GetClientId()}|{pc.FriendCode}|{pc.Data.PlayerName}|{reason}";
        Cloud.SendData(msg);
        Logger.Warn($"EAC报告：{pc.GetRealName()}: {reason}", "EAC Cloud");
    }
    public static bool ReceiveInvalidRpc(PlayerControl pc, byte callId)
    {
        switch (callId)
        {
            case unchecked((byte)42069):
                Report(pc, "AUM");
                HandleCheat(pc, GetString("EAC.CheatDetected.EAC"));
                return true;
            case 101:
                Report(pc, "AUM");
                HandleCheat(pc, GetString("EAC.CheatDetected.EAC"));
                return true;
            //Slok你个歌姬
            case unchecked((byte)520)://YuMenu
                Report(pc, "YM");
                HandleCheat(pc, GetString("EAC.CheatDetected.EAC"));
                return true;
            case 168:
                Report(pc, "SM");
                HandleCheat(pc, GetString("EAC.CheatDetected.EAC"));
                return true;
            case unchecked((byte)420):
                Report(pc, "SM");
                HandleCheat(pc, GetString("EAC.CheatDetected.EAC"));
                return true;
        }
        return true;
    }
    public static void HandleCheat(PlayerControl pc, string text)
    {
        //switch (Options.CheatResponses.GetInt())
        //{
        //    case 0:
        //        Utils.KickPlayer(pc.GetClientId(), true, "CheatDetected");
        //        string msg0 = string.Format(GetString("Message.KickedByEAC"), pc?.Data?.PlayerName, text);
        //        Logger.Warn(msg0, "EAC");
        //        RPC.NotificationPop(msg0);
        //        break;
        //    case 1:
        //        Utils.KickPlayer(pc.GetClientId(), false, "CheatDetected");
        //        string msg1 = string.Format(GetString("Message.BanedByEAC"), pc?.Data?.PlayerName, text);
        //        Logger.Warn(msg1, "EAC");
        //        RPC.NotificationPop(msg1);
        //        break;
        //    case 2:
        //        Utils.SendMessage(string.Format(GetString("Message.NoticeByEAC"), pc?.Data?.PlayerName, text), PlayerControl.LocalPlayer.PlayerId, Utils.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), GetString("MessageFromEAC")));
        //        break;
        //    case 3:
        //        foreach (var apc in Main.AllPlayerControls.Where(x => x.PlayerId != pc?.Data?.PlayerId))
        //            Utils.SendMessage(string.Format(GetString("Message.NoticeByEAC"), pc?.Data?.PlayerName, text), pc.PlayerId, Utils.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), GetString("MessageFromEAC")));
        //        break;
        //}
    }
}