using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Valid;

public class SystemHandler : IRpcHandler
{
    public List<byte> TargetRpcs =>
    [
        (byte)RpcCalls.CloseDoorsOfType,
        (byte)RpcCalls.UpdateSystem
    ];

    public bool HandleLobby(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        return true;
    }

    public bool HandleGame_InMeeting(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        return true;
    }
}