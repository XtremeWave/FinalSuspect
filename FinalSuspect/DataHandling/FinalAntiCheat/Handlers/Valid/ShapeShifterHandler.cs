using System.Collections.Generic;
using AmongUs.GameOptions;
using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using FinalSuspect.Modules.Core.Game;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Valid;

// 46, 55, 56
public class ShapeShifterHandler : IRpcHandler
{
    public List<byte> TargetRpcs =>
    [
        (byte)RpcCalls.Shapeshift,
        (byte)RpcCalls.CheckShapeshift,
        (byte)RpcCalls.RejectShapeshift,
    ];
    
    public bool HandleLobby(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        return true;
    }
    
    public bool HandleGame_All(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        return sender.GetRoleType() is not RoleTypes.Shapeshifter and not RoleTypes.ImpostorGhost;
    }
}