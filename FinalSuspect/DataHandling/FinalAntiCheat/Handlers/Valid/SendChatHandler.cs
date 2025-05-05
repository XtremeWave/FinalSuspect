using System.Collections.Generic;
using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Valid;

// 13
public class SendChatHandler : IRpcHandler
{
    public List<byte> TargetRpcs =>
    [
        (byte)RpcCalls.SendChat,
    ];

    public bool HandleAll(PlayerControl sender, MessageReader reader, 
        ref bool notify, ref string reason, ref bool ban)
    {
        var text = reader.ReadString();
        return text.Length > 100;
    }
    
    public bool HandleGame_InTask(PlayerControl sender, MessageReader reader, 
        ref bool notify, ref string reason, ref bool ban)
    {
        return sender.IsAlive();
    }
}