using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Invalid;

public class NonalusAntiCheatMenu : IRpcHandler
{
    public List<byte> TargetRpcs =>
    [
        unchecked((byte)1337),
        unchecked((byte)5537)
    ];

    public bool HandleInvalidRPC(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        reason = "NonalusAntiCheatMenu";
        return true;
    }
}