using System;
using System.Threading;
using System.Threading.Tasks;
using FinalSuspect.DataHandling.FinalGameData;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using FinalSuspect.Patches.Game_Vanilla;
using Hazel;
using InnerNet;

namespace FinalSuspect.Modules.Features.CheckingandBlocking;

public enum Sounds
{
    KillSound,
    TaskComplete,
    TaskUpdateSound,
    ImpTransform,
    Yeehawfrom
}

[HarmonyPatch]
internal class RPCHandlerPatch
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        return from type in typeof(InnerNetObject).Assembly.GetTypes()
            where typeof(InnerNetObject).IsAssignableFrom(type) && !type.IsAbstract
            select type.GetMethod("HandleRpc", BindingFlags.Public | BindingFlags.Instance)
            into method
            where method != null && method.GetBaseDefinition() != method
            select method;
    }

    public static bool Prefix(InnerNetObject __instance, [HarmonyArgument(0)] ref byte callId,
        [HarmonyArgument(1)] MessageReader reader)
    {
        if (!__instance) return true;

        var player = GetPlayerFromInstance(__instance, reader);
        if (!player) return true;
        if (OnPlayerLeftPatch.ClientsProcessed.Contains(player.PlayerId)) return false;

        if (player.GetCheatData()?.InComingOverloaded != true)
        {
            var cd = player.GetCheatData();
            Info(player.Data
                ? $"{player.Data.PlayerId}(" +
                  $"Name: {player.Data.PlayerName}/" +
                  $"FriendCode: {cd?.FriendCode}/" +
                  $"Puid: {cd?.Puid}" +
                  $")" +
                  $"{(player.IsHost() ? "Host" : "")}:{callId}({RPC.GetRpcName(callId)})"
                : $"Call from {__instance.name}:{callId}({RPC.GetRpcName(callId)})", "ReceiveRPC");
        }

        HandleCheatDetection(player, callId, reader);

        var rpcType = (RpcCalls)callId;
        ProcessRpc(rpcType, player, reader);

        return true;
    }

    private static PlayerControl GetPlayerFromInstance(InnerNetObject instance, MessageReader reader)
    {
        var player = FinalPlayerData.AllPlayerData.FirstOrDefault(x => instance.OwnerId == x.Player.OwnerId)?.Player;
        if (player) return player;

        try
        {
            var sr = MessageReader.Get(reader);
            player = sr.ReadNetObject<PlayerControl>();
        }
        catch
        {
            /* ignored */
        }

        return player;
    }

    private static void HandleCheatDetection(PlayerControl player, byte callId, MessageReader reader)
    {
        if (FinalPlayerData.AllPlayerData.All(data => data.PlayerId != player.Data?.PlayerId)) return;
        if (!ReceiveRpc(player, callId, reader, out var notify, out var reason, out var ban)) return;
        HandleCheater(player, notify, reason, ban, callId);
    }

    private static void HandleCheater(PlayerControl player, bool notify, string reason, bool ban, byte callId)
    {
        if (!player.IsSelf()) player.MarkAsCheater();

        if (AmongUsClient.Instance.AmHost)
        {
            KickPlayer(player.PlayerId, ban, reason, KickLevel.None);
            WarnHost();
            if (notify)
                NotificationPopperPatch.NotificationPop(
                    string.Format(GetString("CheatDetected.InvalidSlothRPC"), player.GetRealName(),
                        $"{callId}({RPC.GetRpcName(callId)})"));
        }
        else if (notify)
        {
            NotificationPopperPatch.NotificationPop(
                string.Format(GetString("CheatDetected.InvalidSlothRPC_NotHost"), player.GetRealName(),
                    $"{callId}({RPC.GetRpcName(callId)})"));
        }
    }

    // 处理RPC调用的逻辑
    private static void ProcessRpc(RpcCalls rpcType, PlayerControl player, MessageReader reader)
    {
        var subReader = MessageReader.Get(reader);

        switch (rpcType)
        {
            case RpcCalls.CheckName:
                HandleCheckNameRpc(player, subReader);
                break;
            case RpcCalls.SetName:
                HandleSetNameRpc(player, subReader);
                break;
            case RpcCalls.SendChat:
                HandleSendChatRpc(player, subReader);
                break;
            case RpcCalls.SendQuickChat:
                HandleSendQuickChatRpc(player);
                break;
            case RpcCalls.StartMeeting:
                HandleStartMeetingRpc(player, subReader);
                break;
        }
    }

    private static void HandleCheckNameRpc(PlayerControl player, MessageReader reader)
    {
        var name = reader.ReadString();
        Info("RPC Check Name For Player: " + name, "CheckName");
        if (player.IsHost())
            FinalGameData.HostNickName = name;
        if (FinalPlayerData.AllPlayerData.All(data => data.PlayerId != player.PlayerId))
            FinalPlayerData.CreateDataFor(player, name);
    }

    private static void HandleSetNameRpc(PlayerControl player, MessageReader reader)
    {
        reader.ReadUInt32();
        var name = reader.ReadString();
        Info("RPC Set Name For Player: " + player.GetNameWithRole() + " => " + name, "SetName");
    }

    private static void HandleSendChatRpc(PlayerControl player, MessageReader reader)
    {
        var text = reader.ReadString();
        Info($"{player.GetNameWithRole()}:{text.RemoveHtmlTags()}", "ReceiveChat");
    }

    private static void HandleSendQuickChatRpc(PlayerControl player)
    {
        Info($"{player.GetNameWithRole()}:Some message from quick chat", "ReceiveChat");
    }

    private static void HandleStartMeetingRpc(PlayerControl player, MessageReader reader)
    {
        var p = GetPlayerById(reader.ReadByte());
        Info($"{player.GetNameWithRole()} => {p?.GetNameWithRole() ?? "null"}", "StartMeeting");
    }

    public static void Postfix(InnerNetObject __instance, [HarmonyArgument(0)] byte callId,
        [HarmonyArgument(1)] MessageReader reader)
    {
        if (!__instance) return;
        var netId = __instance.NetId;
        var player = FinalPlayerData.AllPlayerData.FirstOrDefault(x => x.NetId == netId)?.Player;
        if (!player) return;
        if (FinalGameData.PlayerVersion.PlayerVersions.ContainsKey(player.GetClientId())) return;
        var rpcType = (RpcCalls)callId;
        switch (rpcType)
        {
            case RpcCalls.CancelPet:
                try
                {
                    var version = Version.Parse(reader.ReadString());
                    var tag = reader.ReadString();
                    var forkId = reader.ReadString();

                    var id = player.GetClientId();
                    _ = RPC.RpcVersionCheck();

                    FinalGameData.PlayerVersion.PlayerVersions[id] =
                        new FinalGameData.PlayerVersion(version, tag, forkId);

                    if (ConfigManager.VersionCheat.Value && AmongUsClient.Instance.AmHost)
                        FinalGameData.PlayerVersion.PlayerVersions[id] =
                            FinalGameData.PlayerVersion.PlayerVersions[id];

                    // Kick Unmached Player Start
                    /*if (AmongUsClient.Instance.AmHost && tag != $"{Main.GitCommit}({Main.GitBranch})")
                    {
                        if (forkId != Main.ForkId)
                            _ = new LateTask(() =>
                            {
                                if (__instance?.Data?.Disconnected is not null and not true)
                                {
                                    var msg = string.Format(GetString("KickBecauseDiffrentVersionOrMod"), __instance?.Data?.PlayerName);
                                    Warn(msg, "Version Kick");
                                    NotificationPopperPatch.NotificationPop(msg);
                                    KickPlayer(__instance.GetClientId(), false, "ModVersionIncorrect");
                                }
                            }, 5f, "Kick");
                    }*/
                }
                catch
                {
                    FinalGameData.PlayerVersion.PlayerVersions[player.GetClientId()] = null;
                }

                break;
        }
    }
}

internal static class RPC
{
    private static CancellationTokenSource _rpcCts; // 用于取消异步操作

    public static async Task RpcVersionCheck()
    {
        _rpcCts?.Cancel();
        _rpcCts = new CancellationTokenSource();
        var ct = _rpcCts.Token;

        try
        {
            while (PlayerControl.LocalPlayer == null || AmongUsClient.Instance == null)
            {
                if (ct.IsCancellationRequested) return;
                await Task.Delay(500, ct);
            }

            if (PlayerControl.LocalPlayer == null || AmongUsClient.Instance == null) return;
            if (!ConfigManager.VersionCheat.Value)
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(
                    PlayerControl.LocalPlayer.NetId,
                    (byte)RpcCalls.CancelPet,
                    SendOption.Reliable);
                writer.Write(Main.PluginVersion);
                writer.Write($"{LaunchingInfo.GitCommit}({LaunchingInfo.GitBranch})");
                writer.Write(Main.ForkId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }

            if (FinalGameData.PlayerVersion.PlayerVersions != null)
                FinalGameData.PlayerVersion.PlayerVersions[PlayerControl.LocalPlayer.GetClientId()] =
                    new FinalGameData.PlayerVersion(
                        Version.Parse(Main.PluginVersion),
                        $"{LaunchingInfo.GitCommit}({LaunchingInfo.GitBranch})",
                        Main.ForkId
                    );
        }
        catch (OperationCanceledException)
        {
            /* ignored */
        }
        catch
        {
            /* ignored */
        }
    }
    

    public static void SendRpcLogger(uint targetNetId, byte callId, int targetClientId = -1)
    {
        if (!DebugModeManager.IsDebugMode) return;
        var rpcName = GetRpcName(callId);
        var from = targetNetId.ToString();
        var target = targetClientId.ToString();
        try
        {
            target = targetClientId < 0 ? "All" : AmongUsClient.Instance.GetClient(targetClientId).PlayerName;
            from = Main.AllPlayerControls.FirstOrDefault(c => c.NetId == targetNetId)?.Data?.PlayerName;
        }
        catch
        {
            /* ignored */
        }

        Info($"FromNetID:{targetNetId}({from}) TargetClientID:{targetClientId}({target}) CallID:{callId}({rpcName})",
            "SendRPC");
    }

    public static string GetRpcName(byte callId)
    {
        string rpcName;
        if ((rpcName = Enum.GetName(typeof(RpcCalls), callId)) == null)
            rpcName = callId + "(无效)";
        return rpcName;
    }
}

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.StartRpcImmediately))]
internal class StartRpcImmediatelyPatch
{
    public static void Prefix([HarmonyArgument(0)] uint targetNetId, [HarmonyArgument(1)] byte callId,
        [HarmonyArgument(3)] int targetClientId = -1)
    {
        RPC.SendRpcLogger(targetNetId, callId, targetClientId);
    }
}