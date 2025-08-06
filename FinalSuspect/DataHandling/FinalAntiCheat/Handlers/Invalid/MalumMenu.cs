using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Invalid;

public class MalumMenu : IRpcHandler
{
    public List<byte> TargetRpcs =>
    [
        // Noting
    ];

    public bool HandleInvalidRPC(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        reason = "MalumMenu";
        return true;
    }
}