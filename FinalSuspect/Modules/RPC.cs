using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using System;
using System.Linq;
using System.Threading.Tasks;
using static FinalSuspect.Translator;
using FinalSuspect.Modules.Managers;

namespace FinalSuspect;

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
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] ref byte callId, [HarmonyArgument(1)] MessageReader reader)
    {

        //if (EAC.ReceiveRpc(__instance, callId, reader) && AmongUsClient.Instance.AmHost)
        //{
        //    Utils.KickPlayer(__instance.PlayerId, false, "Hacking");
        //    return false;
        //}

        Logger.Info($"{__instance?.Data?.PlayerId}" +
            $"({__instance?.Data?.PlayerName})" +
            $"({(__instance?.Data?.OwnerId == AmongUsClient.Instance.HostId ? "Host" : "")}" +
            $":{callId}({RPC.GetRpcName(callId)})",
            "ReceiveRPC");

        var rpcType = (RpcCalls)callId;
        MessageReader subReader = MessageReader.Get(reader);
        if (!OnPlayerJoinedPatch.SetNameNum.ContainsKey(__instance.PlayerId))
        {
            OnPlayerJoinedPatch.SetNameNum[__instance.PlayerId] = 0;
        }

        switch (rpcType)
        {
            case RpcCalls.CheckName://CheckNameRPC
                Logger.Info("RPC Cjeck Name For Player: " + __instance.GetNameWithRole(), "CheckName");
                OnPlayerJoinedPatch.SetNameNum[__instance.PlayerId]++;
                break;
            case RpcCalls.SetName: //SetNameRPC
                subReader.ReadUInt32();
                string name = subReader.ReadString();
                Logger.Info("RPC Set Name For Player: " + __instance.GetNameWithRole() + " => " + name, "SetName");
                break;
            case RpcCalls.SetRole: //SetRoleRPC
                var role = (RoleTypes)subReader.ReadUInt16();
                var canOverriddenRole = subReader.ReadBoolean();
                Logger.Info("RPC Set Role For Player: " + __instance.GetRealName() + " => " + role + " CanOverrideRole: " + canOverriddenRole, "SetRole");
                break;
            case RpcCalls.SendChat: // Free chat
                var text = subReader.ReadString();
                Logger.Info($"{__instance.GetNameWithRole().RemoveHtmlTags()}:{text.RemoveHtmlTags()}", "ReceiveChat");
                break;
            case RpcCalls.SendQuickChat:
                Logger.Info($"{__instance.GetNameWithRole().RemoveHtmlTags()}:Some message from quick chat", "ReceiveChat");
                break;
            case RpcCalls.StartMeeting:
                var p = Utils.GetPlayerById(subReader.ReadByte());
                Logger.Info($"{__instance.GetNameWithRole()} => {p?.GetNameWithRole() ?? "null"}", "StartMeeting");
                break;
        }



        if (CheckForInvalidRpc(__instance, callId))
        {
            return false;
        }
        if (CheckForSetName(__instance))
            return false;


        return true;

    }
    static bool CheckForInvalidRpc(PlayerControl player, byte callId)
    {
        if (player.PlayerId != 0 && !Enum.IsDefined(typeof(RpcCalls), callId) && !XtremeGameData.GameStates.OtherModHost)
        {
            Logger.Warn($"{player?.Data?.PlayerName}:{callId}({RPC.GetRpcName(callId)}) 已取消，因为它是由主机以外的其他人发送的。", "CustomRPC");
            if (AmongUsClient.Instance.AmHost)
            {
                if (!EAC.ReceiveInvalidRpc(player, callId)) return true;
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
                Utils.KickPlayer(player.GetClientId(), true, "InvalidRPC");
                Logger.Warn($"收到来自 {player?.Data?.PlayerName} 的多次设置名称，因此将其踢出。", "Kick");
                RPC.NotificationPop(string.Format(GetString("Warning.SetName"), player?.Data?.PlayerName));
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
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        var rpcType = (RpcCalls)callId;
        switch (rpcType)
        {
            case RpcCalls.CancelPet:
                try
                {
                    Version version = Version.Parse(reader.ReadString());
                    string tag = reader.ReadString();
                    string forkId = reader.ReadString();

                    XtremeGameData.PlayerVersion.playerVersion[__instance.PlayerId] = new XtremeGameData.PlayerVersion(version, tag, forkId);

                    if (!Main.VersionCheat.Value) RPC.RpcVersionCheck();

                    if (Main.VersionCheat.Value && AmongUsClient.Instance.AmHost)
                        XtremeGameData.PlayerVersion.playerVersion[__instance.PlayerId] = XtremeGameData.PlayerVersion.playerVersion[0];

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
                {                }
                break;
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
    }
    public static async void RpcVersionCheck()
    {

        while (PlayerControl.LocalPlayer == null) await Task.Delay(500);
        if (!Main.VersionCheat.Value)
        {
            bool cheating = Main.VersionCheat.Value;
            MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.CancelPet, SendOption.Reliable);
            writer.Write(cheating ? XtremeGameData.PlayerVersion.playerVersion[0].version.ToString() : Main.PluginVersion);
            writer.Write(cheating ? XtremeGameData.PlayerVersion.playerVersion[0].tag : $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})");
            writer.Write(cheating ? XtremeGameData.PlayerVersion.playerVersion[0].forkId : Main.ForkId);
            writer.EndMessage();
        }
        XtremeGameData.PlayerVersion.playerVersion[PlayerControl.LocalPlayer.PlayerId] = new XtremeGameData.PlayerVersion(Main.PluginVersion, $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})", Main.ForkId);


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
        if ((rpcName = Enum.GetName(typeof(RpcCalls), callId)) == null)
            rpcName = callId.ToString() + "INVALID";
        return rpcName;
    }
    public static void NotificationPop(string text)
    {
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