using AmongUs.Data;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Hazel;
using InnerNet;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FinalSuspect.Modules;



using static FinalSuspect.Translator;
using FinalSuspect.Modules.Managers;
using System.Linq;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using static Il2CppSystem.Globalization.CultureInfo;

namespace FinalSuspect;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
class OnGameJoinedPatch
{
    public static void Postfix(AmongUsClient __instance)
    {
        HudManagerPatch.Init();
        FAC.SetNameNum = new();
        Logger.Info($"{__instance.GameId} 加入房间", "OnGameJoined");
        XtremeGameData.PlayerVersion.playerVersion = new Dictionary<byte, XtremeGameData.PlayerVersion>();

        SoundManager.Instance.ChangeAmbienceVolume(DataManager.Settings.Audio.AmbienceVolume);

        if (!Main.VersionCheat.Value) RPC.RpcVersionCheck();
        XtremeGameData.GameStates.InGame = false;
        ErrorText.Instance.Clear();
        ServerAddManager.SetServerName();

        FAC.Init();
        if (AmongUsClient.Instance.AmHost) 
        {
            GameStartManagerPatch.GameStartManagerUpdatePatch.exitTimer = -1;
            Main.NewLobby = true;
        }



    }
}
[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.DisconnectInternal))]
class DisconnectInternalPatch
{
    public static void Prefix(InnerNetClient __instance, DisconnectReasons reason, string stringReason)
    {
        try
        {
            ShowDisconnectPopupPatch.Reason = reason;
            ShowDisconnectPopupPatch.StringReason = stringReason;

            Logger.Info($"断开连接(理由:{reason}:{stringReason}，Ping:{__instance.Ping})", "Session");
            HudManagerPatch.Init();
            XtremeGameData.XtremePlayerData.AllPlayerData.Values.ToArray().Do(data => data.Dispose());

            ErrorText.Instance.CheatDetected = false;
            ErrorText.Instance.SBDetected = false;
            ErrorText.Instance.Clear();
            Cloud.StopConnect();

        }
        catch { }
    }
}
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
public class OnPlayerJoinedPatch
{
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
    {
        Logger.Info($"{client.PlayerName}(ClientID:{client.Id}/FriendCode:{client.FriendCode}) 加入房间", "Session");
        if (AmongUsClient.Instance.AmHost && client.FriendCode == "" && Main.KickPlayerFriendCodeNotExist.Value)
        {
            Utils.KickPlayer(client.Id, false, "NotLogin");
            NotificationPopperPatch.NotificationPop(string.Format(GetString("Message.KickedByNoFriendCode"), client.PlayerName));
            Logger.Info($"フレンドコードがないプレイヤーを{client?.PlayerName}をキックしました。", "Kick");
        }
        if (DestroyableSingleton<FriendsListManager>.Instance.IsPlayerBlockedUsername(client.FriendCode) && AmongUsClient.Instance.AmHost && Main.ApplyBanList.Value)
        {
            Utils.KickPlayer(client.Id, true, "BanList");
            Logger.Info($"ブロック済みのプレイヤー{client?.PlayerName}({client.FriendCode})をBANしました。", "BAN");
        }
        BanManager.CheckBanPlayer(client);
        BanManager.CheckDenyNamePlayer(client);

        RPC.RpcVersionCheck();

    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
class OnPlayerLeftPatch
{
    public static List<int> ClientsProcessed = new();
    public static void Add(int id)
    {
        ClientsProcessed.Remove(id);
        ClientsProcessed.Add(id);
    }
    public static void Prefix([HarmonyArgument(0)] ClientData data)
    {
        if (AmongUsClient.Instance.AmHost && XtremeGameData.GameStates.IsOnlineGame && data.Character != null)
        {
            var netid = data.Character.NetId;
            _ = new LateTask(() =>
            {
                MessageWriter messageWriter = AmongUsClient.Instance.Streams[1];
                messageWriter.StartMessage(5);
                messageWriter.WritePacked(netid);
                messageWriter.EndMessage();
            }, 2.5f, "Repeat Despawn");
        }

    }
    public static void Postfix([HarmonyArgument(0)] ClientData data, [HarmonyArgument(1)] DisconnectReasons reason)
    {
        try
        {
            if (data == null)
            {
                Logger.Error("错误的客户端数据：数据为空", "Session");
                return;
            }

            if (XtremeGameData.GameStates.IsInGame)
            {
                data?.Character?.SetDisconnected();
            }

            Logger.Info($"{data?.PlayerName}(ClientID:{data?.Id}/FriendCode:{data?.FriendCode})断开连接(理由:{reason}，Ping:{AmongUsClient.Instance.Ping})", "Session");

            // 附加描述掉线原因
            switch (reason)
            {
                case DisconnectReasons.Hacking:
                    NotificationPopperPatch.NotificationPop(string.Format(GetString("PlayerLeftByAU-Anticheat"), data?.PlayerName));
                    break;
                case DisconnectReasons.Error:
                    NotificationPopperPatch.NotificationPop(string.Format(GetString("PlayerLeftCuzError"), data?.PlayerName));
                    break;
                case DisconnectReasons.Kicked:
                case DisconnectReasons.Banned:
                    break;
                default:
                    if (!ClientsProcessed.Contains(data?.Id ?? 0))
                        NotificationPopperPatch.NotificationPop(string.Format(GetString("PlayerLeft"), data?.PlayerName));
                    break;
            }
            XtremeGameData.PlayerVersion.playerVersion.Remove(data?.Character?.PlayerId ?? 255);
            ClientsProcessed.Remove(data?.Id ?? 0);
            FAC.SetNameNum.Remove(data?.Character?.PlayerId ?? 255);
        }
        catch { }
    }
}