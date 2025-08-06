using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Invalid;

public class SickoMenu : IRpcHandler
{
    public List<byte> TargetRpcs =>
    [
        168,
        unchecked((byte)420)
    ];

    public bool HandleInvalidRPC(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        reason = "SickoMenu";
        return true;
    }
}