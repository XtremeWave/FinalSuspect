using System;
using System.Linq;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using AmongUs.QuickChat;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game;
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
    Yeehawfrom,
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
internal class RPCHandlerPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] ref byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        XtremeLogger.Info($"{__instance?.Data?.PlayerId}" +
                          $"({__instance?.Data?.PlayerName})" +
                          $"{(__instance.IsHost() ? "Host" : "")}" +
                          $":{callId}({RPC.GetRpcName(callId)})",
            "ReceiveRPC");

        if (XtremePlayerData.AllPlayerData.Any(data => data.PlayerId == __instance?.Data?.PlayerId))
            if (FinalAntiCheat.FAC.ReceiveRpc(__instance, callId, reader, out var notify, out var reason, out var ban))
            {
                if (!__instance.IsLocalPlayer())
                {
                    __instance.MarkAsCheater();
                }
                if (AmongUsClient.Instance.AmHost)
                {
                    Utils.KickPlayer(__instance.PlayerId, ban, reason);
                    FinalAntiCheat.FAC.WarnHost();
                    if (notify)
                        NotificationPopperPatch.NotificationPop
                            (string.Format(GetString("Warning.InvalidSlothRPC"), __instance.GetRealName(), $"{callId}({RPC.GetRpcName(callId)})"));
                }
                else if (notify)
                    NotificationPopperPatch.NotificationPop
                        (string.Format(GetString("Warning.InvalidSlothRPC_NotHost"), __instance.GetRealName(), $"{callId}({RPC.GetRpcName(callId)})"));
                return false;
            }

        var rpcType = (RpcCalls)callId;
        var subReader = MessageReader.Get(reader);

        switch (rpcType)
        {
            case RpcCalls.CheckName://CheckNameRPC
                var name = subReader.ReadString();
                XtremeLogger.Info("RPC Check Name For Player: " + name, "CheckName");
                if (__instance.IsHost())
                    Main.HostNickName = name;
                if (XtremePlayerData.AllPlayerData.All(data => data.PlayerId != __instance.PlayerId))
                    XtremePlayerData.CreateDataFor(__instance, name);
                break;
            case RpcCalls.SetName: //SetNameRPC
                subReader.ReadUInt32();
                name = subReader.ReadString();
                XtremeLogger.Info("RPC Set Name For Player: " + __instance.GetNameWithRole() + " => " + name, "SetName");
                break;
            case RpcCalls.SetRole: //SetRoleRPC
                var role = (RoleTypes)subReader.ReadUInt16();
                var canOverriddenRole = subReader.ReadBoolean();
                XtremeLogger.Info("RPC Set Role For Player: " + __instance.GetRealName() + " => " + role + " CanOverrideRole: " + canOverriddenRole, "SetRole");
                break;
            case RpcCalls.SendChat: // Free chat
                var text = subReader.ReadString();
                XtremeLogger.Info($"{__instance.GetNameWithRole().RemoveHtmlTags()}:{text.RemoveHtmlTags()}", "ReceiveChat");
                break;
            case RpcCalls.SendQuickChat:
                XtremeLogger.Info($"{__instance.GetNameWithRole().RemoveHtmlTags()}:Some message from quick chat", "ReceiveChat");
                break;
            case RpcCalls.StartMeeting:
                var p = Utils.GetPlayerById(subReader.ReadByte());
                XtremeLogger.Info($"{__instance.GetNameWithRole()} => {p?.GetNameWithRole() ?? "null"}", "StartMeeting");
                break;
        }
        return true;
    }
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        var rpcType = (RpcCalls)callId;
        switch (rpcType)
        {
            case RpcCalls.CancelPet:
                try
                {
                    var version = Version.Parse(reader.ReadString());
                    var tag = reader.ReadString();
                    var forkId = reader.ReadString();

                    XtremeGameData.PlayerVersion.playerVersion[__instance.PlayerId] = new XtremeGameData.PlayerVersion(version, tag, forkId);

                    if (!XtremeGameData.PlayerVersion.playerVersion.ContainsKey(__instance.PlayerId))
                        RPC.RpcVersionCheck();

                    if (Main.VersionCheat.Value && AmongUsClient.Instance.AmHost)
                        XtremeGameData.PlayerVersion.playerVersion[__instance.PlayerId] = XtremeGameData.PlayerVersion.playerVersion[0];

                    // Kick Unmached Player Start
                    /*if (AmongUsClient.Instance.AmHost && tag != $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})")
                    {
                        if (forkId != Main.ForkId)
                            _ = new LateTask(() =>
                            {
                                if (__instance?.Data?.Disconnected is not null and not true)
                                {
                                    var msg = string.Format(GetString("KickBecauseDiffrentVersionOrMod"), __instance?.Data?.PlayerName);
                                    XtremeLogger.Warn(msg, "Version Kick");
                                    NotificationPopperPatch.NotificationPop(msg);
                                    Utils.KickPlayer(__instance.GetClientId(), false, "ModVersionIncorrect");
                                }
                            }, 5f, "Kick");
                    }*/
                }
                catch
                {                }
                break;
        }
    }

}

internal static class RPC
{
    public static async void RpcVersionCheck()
    {
        try
        {
            while (PlayerControl.LocalPlayer == null) await Task.Delay(500);
            if (!Main.VersionCheat.Value)
            {
                var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.CancelPet);
                writer.Write(Main.PluginVersion);
                writer.Write($"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})");
                writer.Write(Main.ForkId);
                writer.EndMessage();
            }
            XtremeGameData.PlayerVersion.playerVersion[PlayerControl.LocalPlayer.PlayerId] = 
                new XtremeGameData.PlayerVersion(Main.PluginVersion, $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})", Main.ForkId);
        }
        catch 
        {
        }
    }


    public static void SendRpcLogger(uint targetNetId, byte callId, int targetClientId = -1)
    {
        if (!DebugModeManager.AmDebugger) return;
        var rpcName = GetRpcName(callId);
        var from = targetNetId.ToString();
        var target = targetClientId.ToString();
        try
        {
            target = targetClientId < 0 ? "All" : AmongUsClient.Instance.GetClient(targetClientId).PlayerName;
            from = Main.AllPlayerControls.Where(c => c.NetId == targetNetId).FirstOrDefault()?.Data?.PlayerName;
        }
        catch { }
        XtremeLogger.Info($"FromNetID:{targetNetId}({from}) TargetClientID:{targetClientId}({target}) CallID:{callId}({rpcName})", "SendRPC");
    }
    public static string GetRpcName(byte callId)
    {
        string rpcName;
        if ((rpcName = Enum.GetName(typeof(RpcCalls), callId)) == null)
            rpcName = callId + "INVALID";
        return rpcName;
    }
}
[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.StartRpc))]
internal class StartRpcPatch
{
    public static void Prefix(InnerNetClient __instance, [HarmonyArgument(0)] uint targetNetId, [HarmonyArgument(1)] byte callId)
    {
            RPC.SendRpcLogger(targetNetId, callId);
    }
}
[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.StartRpcImmediately))]
internal class StartRpcImmediatelyPatch
{
    public static void Prefix(InnerNetClient __instance, [HarmonyArgument(0)] uint targetNetId, [HarmonyArgument(1)] byte callId, [HarmonyArgument(3)] int targetClientId = -1)
    {
            RPC.SendRpcLogger(targetNetId, callId, targetClientId);
    }
}