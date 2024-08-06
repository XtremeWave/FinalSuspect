using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static FinalSuspect_Xtreme.Translator;
using FinalSuspect_Xtreme.Modules.SoundInterface;
namespace FinalSuspect_Xtreme;

public enum CustomRPC
{
    VersionCheck = 99,
    RequestRetryVersionCheck = 100,
}
public enum Sounds
{
    KillSound,
    TaskComplete,
    TaskUpdateSound,
    ImpTransform,
    Yeehawfrom,
    CopyCode,
    Test,
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
internal class RPCHandlerPatch
{
    public static bool TrustedRpc(byte id)
 => (CustomRPC)id is CustomRPC.VersionCheck or CustomRPC.RequestRetryVersionCheck /*or  CustomRPC.PlaySound */;

    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {

        if (EAC.ReceiveRpc(__instance, callId, reader)) return false;

        Logger.Info($"{__instance?.Data?.PlayerId}({(__instance?.Data?.OwnerId == AmongUsClient.Instance.HostId ? "Host" : __instance?.Data?.PlayerName)}):{callId}({RPC.GetRpcName(callId)})", "ReceiveRPC");


        if (__instance.PlayerId != 0 && !Enum.IsDefined(typeof(RpcCalls), callId) && !TrustedRpc(callId))
        {
            Logger.Warn($"{__instance?.Data?.PlayerName}:{callId}({RPC.GetRpcName(callId)}) 已取消，因为它是由主机以外的其他人发送的。", "CustomRPC");
            if (AmongUsClient.Instance.AmHost)
            {
                if (!EAC.ReceiveInvalidRpc(__instance, callId)) return false;
                Utils.KickPlayer(__instance.GetClientId(), false, "InvalidRPC");
                Logger.Warn($"收到来自 {__instance?.Data?.PlayerName} 的不受信用的RPC，因此将其踢出。", "Kick");
                RPC.NotificationPop(string.Format(GetString("Warning.InvalidRpc"), __instance?.Data?.PlayerName));
            }
            else if (!Main.playerVersion.TryGetValue(0, out _))
            {
                Logger.Warn($"收到来自 {__instance?.Data?.PlayerName} 的不受信用的RPC", "Kick?");
                RPC.NotificationPop(string.Format(GetString("Warning.InvalidRpc_NotHost"), __instance?.Data?.PlayerName, callId));
            }
            return false;
        }

        return true;
        
    }
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {

        if ((CustomRPC)callId is not CustomRPC.VersionCheck and not CustomRPC.RequestRetryVersionCheck) return;

        var rpcType = (CustomRPC)callId;
        switch (rpcType)
        {
            case CustomRPC.VersionCheck:
                try
                {
                    Version version = Version.Parse(reader.ReadString());
                    string tag = reader.ReadString();
                    string forkId = reader.ReadString();
                    Main.playerVersion[__instance.PlayerId] = new PlayerVersion(version, tag, forkId);

                    if (Main.VersionCheat.Value && __instance.OwnerId == AmongUsClient.Instance.HostId) RPC.RpcVersionCheck();

                    if (Main.VersionCheat.Value && AmongUsClient.Instance.AmHost)
                        Main.playerVersion[__instance.PlayerId] = Main.playerVersion[0];

                    // Kick Unmached Player Start
                    if (AmongUsClient.Instance.AmHost && tag != $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})")
                    {
                        if (forkId != Main.ForkId)
                            _ = new LateTask(() =>
                            {
                                if (__instance?.Data?.Disconnected is not null and not true)
                                {
                                    var msg = string.Format(GetString("KickBecauseDiffrentVersionOrMod"), __instance?.Data?.PlayerName);
                                    Logger.Warn(msg, "Version Kick");
                                    RPC.NotificationPop(msg);
                                    Utils.KickPlayer(__instance.GetClientId(), false, "ModVersionIncorrect");
                                }
                            }, 5f, "Kick");
                    }
                    // Kick Unmached Player End
                }
                catch
                {
                    Logger.Warn($"{__instance?.Data?.PlayerName}({__instance.PlayerId}): バージョン情報が無効です", "RpcVersionCheck");
                    _ = new LateTask(() =>
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RequestRetryVersionCheck, SendOption.Reliable, __instance.GetClientId());
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                    }, 1f, "Retry Version Check Task");
                }
                break;
            case CustomRPC.RequestRetryVersionCheck:
                RPC.RpcVersionCheck();
                break;
            //case CustomRPC.PlaySound:
            //    byte playerID = reader.ReadByte();
            //    Sounds sound = (Sounds)reader.ReadByte();
            //    RPC.PlaySound(playerID, sound);
            //    break;
            //case CustomRPC.ShowPopUp:
            //    string msg = reader.ReadString();
            //    HudManager.Instance.ShowPopUp(msg);
            //    break;
            //case CustomRPC.PlayCustomSound:
            //    CustomSoundsManager.ReceiveRPC(reader);
            //    break;
            //case CustomRPC.NotificationPop:
            //    NotificationPopperPatch.AddItem(reader.ReadString());
            //    break;
            //case CustomRPC.SetKickReason:
            //    ShowDisconnectPopupPatch.ReasonByHost = reader.ReadString();
            //    break;
        }
    }
}

internal static class RPC
{
    //来源：https://github.com/music-discussion/TownOfHost-TheOtherRoles/blob/main/Modules/RPC.cs
    public static void PlaySoundRPC(byte PlayerID, Sounds sound)
    {
        if (AmongUsClient.Instance.AmHost)
            PlaySound(PlayerID, sound);
        //MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlaySound, SendOption.Reliable, -1);
        //writer.Write(PlayerID);
        //writer.Write((byte)sound);
        //AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    //public static void ShowPopUp(this PlayerControl pc, string msg)
    //{
    //    if (!AmongUsClient.Instance.AmHost) return;
    //    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShowPopUp, SendOption.Reliable, pc.GetClientId());
    //    writer.Write(msg);
    //    AmongUsClient.Instance.FinishRpcImmediately(writer);
    //}
    public static async void RpcVersionCheck()
    {


        if (!AmongUsClient.Instance.AmHost && Main.playerVersion.TryGetValue(0, out var ver) && Main.ForkId != ver.forkId) return;

        while (PlayerControl.LocalPlayer == null) await Task.Delay(500);
        if (Main.playerVersion.ContainsKey(0) || !Main.VersionCheat.Value)
        {
            bool cheating = Main.VersionCheat.Value;
            MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VersionCheck, SendOption.Reliable);
            writer.Write(cheating ? Main.playerVersion[0].version.ToString() : Main.PluginVersion);
            writer.Write(cheating ? Main.playerVersion[0].tag : $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})");
            writer.Write(cheating ? Main.playerVersion[0].forkId : Main.ForkId);
            writer.EndMessage();
        }
        Main.playerVersion[PlayerControl.LocalPlayer.PlayerId] = new PlayerVersion(Main.PluginVersion, $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})", Main.ForkId);


    }
    public static void PlaySound(byte playerID, Sounds sound)
    {
        if (PlayerControl.LocalPlayer.PlayerId == playerID)
        {
            switch (sound)
            {
                case Sounds.KillSound:
                    SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.KillSfx, false, 1f);
                    break;
                case Sounds.TaskComplete:
                    SoundManager.Instance.PlaySound(DestroyableSingleton<HudManager>.Instance.TaskCompleteSound, false, 1f);
                    break;
                case Sounds.TaskUpdateSound:
                    SoundManager.Instance.PlaySound(DestroyableSingleton<HudManager>.Instance.TaskUpdateSound, false, 1f);
                    break;
                case Sounds.ImpTransform:
                    SoundManager.Instance.PlaySound(DestroyableSingleton<HnSImpostorScreamSfx>.Instance.HnSOtherImpostorTransformSfx, false, 0.8f);
                    break;
                case Sounds.Yeehawfrom:
                    SoundManager.Instance.PlaySound(DestroyableSingleton<HnSImpostorScreamSfx>.Instance.HnSLocalYeehawSfx, false, 0.8f);
                    break;
                case Sounds.CopyCode:
                    SoundManager.Instance.PlaySoundImmediate(DestroyableSingleton<LobbyInfoPane>.Instance.CopyCodeSound, false, 1f);
                    break;
            }
        }
    }


    public static void SendRpcLogger(uint targetNetId, byte callId, int targetClientId = -1)
    {
        if (!DebugModeManager.AmDebugger) return;
        string rpcName = GetRpcName(callId);
        string from = targetNetId.ToString();
        string target = targetClientId.ToString();
        try
        {
            target = targetClientId < 0 ? "All" : AmongUsClient.Instance.GetClient(targetClientId).PlayerName;
            from = Main.AllPlayerControls.Where(c => c.NetId == targetNetId).FirstOrDefault()?.Data?.PlayerName;
        }
        catch { }
        Logger.Info($"FromNetID:{targetNetId}({from}) TargetClientID:{targetClientId}({target}) CallID:{callId}({rpcName})", "SendRPC");
    }
    public static string GetRpcName(byte callId)
    {
        string rpcName;
        if ((rpcName = Enum.GetName(typeof(RpcCalls), callId)) != null) { }
        else if ((rpcName = Enum.GetName(typeof(CustomRPC), callId)) != null) { }
        else rpcName = callId.ToString() + "(INVALID)";
        return rpcName;
    }
    public static void NotificationPop(string text)
    {
        //MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.NotificationPop, Hazel.SendOption.Reliable, -1);
        //writer.Write(text);
        //AmongUsClient.Instance.FinishRpcImmediately(writer);
        NotificationPopperPatch.AddItem(text);
    }
}
[HarmonyPatch(typeof(InnerNet.InnerNetClient), nameof(InnerNet.InnerNetClient.StartRpc))]
internal class StartRpcPatch
{
    public static void Prefix(InnerNet.InnerNetClient __instance, [HarmonyArgument(0)] uint targetNetId, [HarmonyArgument(1)] byte callId)
    {
            RPC.SendRpcLogger(targetNetId, callId);
    }
}
[HarmonyPatch(typeof(InnerNet.InnerNetClient), nameof(InnerNet.InnerNetClient.StartRpcImmediately))]
internal class StartRpcImmediatelyPatch
{
    public static void Prefix(InnerNet.InnerNetClient __instance, [HarmonyArgument(0)] uint targetNetId, [HarmonyArgument(1)] byte callId, [HarmonyArgument(3)] int targetClientId = -1)
    {
            RPC.SendRpcLogger(targetNetId, callId, targetClientId);
    }
}