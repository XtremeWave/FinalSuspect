using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Valid;

// 39~43
public class OutfitStrHandler : IRpcHandler
{
    public List<byte> TargetRpcs =>
    [
        (byte)RpcCalls.SetHatStr,
        (byte)RpcCalls.SetSkinStr,
        (byte)RpcCalls.SetPetStr,
        (byte)RpcCalls.SetVisorStr,
        (byte)RpcCalls.SetNamePlateStr
    ];

    public bool HandleGame_All(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        return true;
    }
}