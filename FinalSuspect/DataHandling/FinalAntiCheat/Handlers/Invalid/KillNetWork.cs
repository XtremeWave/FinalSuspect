using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Invalid;

public class KillNetWork : IRpcHandler
{
    public List<byte> TargetRpcs =>
    [
        119,
        250
    ];

    public bool HandleInvalidRPC(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        reason = "KillNetWork";
        return true;
    }
}