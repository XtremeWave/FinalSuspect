using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Invalid;

public class BanMod : IRpcHandler
{
    public List<byte> TargetRpcs =>
    [
        201,
        202,
        203,
        204,
        205
    ];

    public bool HandleInvalidRPC(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        reason = "BanMod";
        return false;
    }
}