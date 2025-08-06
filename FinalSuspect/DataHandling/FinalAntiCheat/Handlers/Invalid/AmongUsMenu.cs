using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Invalid;

public class AmongUsMenu : IRpcHandler
{
    public List<byte> TargetRpcs =>
    [
        unchecked((byte)42069),
        101
    ];

    public bool HandleInvalidRPC(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        reason = "AmongUsMenu";
        return true;
    }
}