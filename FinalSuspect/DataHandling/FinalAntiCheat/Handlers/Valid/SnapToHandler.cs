using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Valid;

public class SnapToHandler : IRpcHandler
{
    public List<byte> TargetRpcs =>
    [
        (byte)RpcCalls.SnapTo
    ];

    public int MaxiReceivedNumPerSecond()
    {
        return 30;
    }
}