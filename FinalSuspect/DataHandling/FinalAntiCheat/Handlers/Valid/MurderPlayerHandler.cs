using System.Collections.Generic;
using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using FinalSuspect.Modules.Core.Game;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Valid;

// 12, 47
public class MurderPlayerHandler : IRpcHandler
{
    public List<byte> TargetRpcs =>
    [
        (byte)RpcCalls.MurderPlayer,
        (byte)RpcCalls.CheckMurder,
    ];

    public bool HandleGame_InTask(PlayerControl sender, MessageReader reader, 
        ref bool notify, ref string reason, ref bool ban)
    {
        return !sender.IsImpostor();
    }

    public bool HandleLobby(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        return true;
    }
}