using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Valid;

// 7, 8
public class SetColorHandler : IRpcHandler
{
    public List<byte> TargetRpcs =>
    [
        (byte)RpcCalls.CheckColor,
        (byte)RpcCalls.SetColor
    ];

    public bool HandleGame_All(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        return true;
    }
}