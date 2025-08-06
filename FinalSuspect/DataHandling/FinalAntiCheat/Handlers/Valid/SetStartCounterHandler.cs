using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Valid;

// 18
public class SetStartCounterHandler : IRpcHandler
{
    public List<byte> TargetRpcs =>
    [
        (byte)RpcCalls.SetStartCounter
    ];

    public bool HandleGame_All(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        return true;
    }

    public int MaxiReceivedNumPerSecond() => 20;
}