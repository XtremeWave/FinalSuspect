using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Invalid;

public class VersionCheck : IRpcHandler
{
    public List<byte> TargetRpcs =>
    [
        80
    ];

    public bool HandleInvalidRPC(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        reason = "VersionCheck(Host Only Mod)";
        return false;
    }
}