using System;
using System.Collections.Generic;
using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Valid;

// 33
public class SendQuickChatHandler : IRpcHandler
{
    private static readonly Dictionary<byte, (long timestamp, int count)> _records = new();
    
    public List<byte> TargetRpcs =>
    [
        (byte)RpcCalls.SendQuickChat,
    ];

    public bool HandleAll(PlayerControl sender, MessageReader reader, 
        ref bool notify, ref string reason, ref bool ban)
    {
        var current = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (!_records.TryGetValue(sender.PlayerId, out var record))
        {
            record = (current, 0);
        }

        if (current - record.timestamp < 3)
        {
            record.count++;
            if (record.count > 1)
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    HandleCheat(sender, GetString("Warning.SendQuickChat"));
                }
                else if (!OtherModHost)
                {
                    HandleCheat(sender, GetString("Warning.SendQuickChat_NotHost"));
                }
                Warn($"{sender.GetDataName()}({sender.GetCheatData().FriendCode})({sender.GetCheatData().Puid})一秒内多次发送快捷消息", "FAC");
                ban = true;
                notify = false;
                return true;
            }
        }
        else
        {
            record = (current, 0);
        }

        _records[sender.PlayerId] = record;
        return false;
    }
    
    public bool HandleGame_InTask(PlayerControl sender, MessageReader reader, 
        ref bool notify, ref string reason, ref bool ban)
    {
        return sender.IsAlive();
    }
}