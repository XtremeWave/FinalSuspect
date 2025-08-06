using System;
using AmongUs.InnerNet.GameDataMessages;
using FinalSuspect.DataHandling.FinalGameData;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using Hazel;
using InnerNet;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleGameData))]
public static class HandleGameDataPatch
{
    public static bool Prefix(InnerNetClient __instance, [HarmonyArgument(0)] MessageReader parentReader)
    {
        if (!IsLobby || IsNotJoined || !FinalGameData.JoinedCompleted || !Main.EnableGuardian.Value) return true;

        try
        {
            if (parentReader.BytesRemaining < 1) return false;
            var messageReader = MessageReader.Get(parentReader);
            var reader = messageReader.ReadMessageAsNewBuffer();
            return reader.BytesRemaining >= 1;
        }
        catch
        {
            return PlayerControl.LocalPlayer?.GetClient() == null;
        }
    }
}

[HarmonyPatch(typeof(InnerNetClient._HandleGameDataInner_d__165),
    nameof(InnerNetClient._HandleGameDataInner_d__165.MoveNext))]
public static class HandleGameDataInnerPatch
{
    private static readonly Dictionary<int, MsgCounter> playerMsgCounters = new();

    public static bool Prefix(InnerNetClient._HandleGameDataInner_d__165 __instance)
    {
        if (!IsLobby || IsNotJoined || !FinalGameData.JoinedCompleted || !Main.EnableGuardian.Value) return true;
        var reader = __instance.reader;
        if (reader.BytesRemaining < 1)
        {
            reader.Recycle();
            return false;
        }

        var innerNetClient = __instance.__4__this;
        var tag = (GameDataTypes)reader.Tag;
        var sr = MessageReader.Get(reader);
        InnerNetObject targetObject = null;
        ClientData clientData = null;
        try
        {
            switch (tag)
            {
                case GameDataTypes.DataFlag:
                case GameDataTypes.RpcFlag:
                    var netId = sr.ReadPackedUInt32();
                    lock (innerNetClient.allObjects)
                    {
                        innerNetClient.allObjects.AllObjectsFast.TryGetValue(netId, out targetObject);
                    }

                    if (tag == GameDataTypes.RpcFlag)
                        sr.ReadByte();
                    break;
                case GameDataTypes.ReadyFlag:
                    clientData = innerNetClient.FindClientById(sr.ReadPackedInt32());
                    break;
                case GameDataTypes.SceneChangeFlag:
                    var clientId = sr.ReadPackedInt32();
                    clientData = innerNetClient.FindClientById(clientId);
                    break;
                case GameDataTypes.SpawnFlag:
                case GameDataTypes.DespawnFlag:
                case GameDataTypes.XboxDeclareXuid:
                    sr.Recycle();
                    return true;
                default:
                    sr.Recycle();
                    reader.Recycle();
                    return false;
            }
        }
        catch
        {
            /* ignored */
        }

        if (clientData == null && targetObject == null || sr.BytesRemaining < 0)
        {
            sr.Recycle();
            reader.Recycle();
            return false;
        }

        var ownerId = targetObject?.OwnerId;
        if (!FinalPlayerData.AllPlayerData.Any(x => x.Player.OwnerId == ownerId))
        {
            sr.Recycle();
            return true;
        }

        var client = innerNetClient.FindClientById(ownerId ?? -1232242121); // 瞎写八写保证为null
        clientData ??= client;
        if (clientData == null)
        {
            sr.Recycle();
            reader.Recycle();
            return false;
        }

        if (!playerMsgCounters.TryGetValue(clientData.Id, out var counter))
        {
            counter = new MsgCounter();
            playerMsgCounters[clientData.Id] = counter;
        }

        if (counter.IncomingOverload) return false;

        counter.Update(reader.Tag);

        if (counter.TotalMsgLastSecond <= 100 && counter.GetRpcCount(reader.Tag) <= 60)
        {
            sr.Recycle();
            return true;
        }

        counter.IncomingOverload = true;
        var _player = FinalPlayerData.AllPlayerData.FirstOrDefault(x => x.CheatData.ClientData.Id == clientData.Id)
            ?.Player;
        Warn($"Incoming Msg Overloaded: {_player?.GetDataName() ?? ""}", "FAC");
        _player?.MarkAsHacker();
        sr.Recycle();
        reader.Recycle();
        return false;
    }

    private class MsgCounter
    {
        private readonly Dictionary<byte, int> MsgTypeCounts = new();
        public bool IncomingOverload;
        private DateTime lastReset = DateTime.UtcNow;
        public int TotalMsgLastSecond;

        public void Update(byte rpcType)
        {
            if ((DateTime.UtcNow - lastReset).TotalSeconds >= 1)
            {
                TotalMsgLastSecond = 0;
                MsgTypeCounts.Clear();
                lastReset = DateTime.UtcNow;
            }

            TotalMsgLastSecond++;
            MsgTypeCounts[rpcType] = MsgTypeCounts.TryGetValue(rpcType, out var count) ? count + 1 : 1;
        }

        public int GetRpcCount(byte rpcType)
        {
            return MsgTypeCounts.GetValueOrDefault(rpcType, 0);
        }
    }
}

[HarmonyPatch(typeof(InnerNetServer), nameof(InnerNetServer.HandleMessage))]
internal class HandleMessagePatch
{
    private static readonly Dictionary<int, MsgCounter> playerMsgCounters = new();

    public static bool Prefix(InnerNetServer.Player client, MessageReader reader)
    {
        if (!IsLobby || IsNotJoined || !FinalGameData.JoinedCompleted || !Main.EnableGuardian.Value) return true;

        if (!playerMsgCounters.TryGetValue(client.Id, out var counter))
        {
            counter = new MsgCounter();
            playerMsgCounters[client.Id] = counter;
        }

        if (counter.IncomingOverload) return false;
        counter.Update(reader.Tag);

        if (counter.TotalMsgLastSecond <= 100 && counter.GetRpcCount(reader.Tag) <= 60) return true;

        counter.IncomingOverload = true;
        var _player = FinalPlayerData.AllPlayerData.FirstOrDefault(x => x.CheatData.ClientData.Id == client.Id)
            ?.Player;
        Warn($"Incoming Msg Overloaded: {_player?.GetDataName() ?? ""}", "FAC");
        _player?.MarkAsHacker();
        return false;
    }

    private class MsgCounter
    {
        private readonly Dictionary<byte, int> MsgTypeCounts = new();
        public bool IncomingOverload;
        private DateTime lastReset = DateTime.UtcNow;
        public int TotalMsgLastSecond;

        public void Update(byte rpcType)
        {
            if ((DateTime.UtcNow - lastReset).TotalSeconds >= 1)
            {
                TotalMsgLastSecond = 0;
                MsgTypeCounts.Clear();
                lastReset = DateTime.UtcNow;
            }

            TotalMsgLastSecond++;

            MsgTypeCounts[rpcType] = MsgTypeCounts.TryGetValue(rpcType, out var count) ? count + 1 : 1;
        }

        public int GetRpcCount(byte rpcType)
        {
            return MsgTypeCounts.GetValueOrDefault(rpcType, 0);
        }
    }
}