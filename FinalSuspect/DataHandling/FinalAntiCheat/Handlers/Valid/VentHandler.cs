using System.Collections.Generic;
using AmongUs.GameOptions;
using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using FinalSuspect.Modules.Core.Game;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Valid;

// 19, 20
public class VentHandler : IRpcHandler
{
    public List<byte> TargetRpcs =>
    [
        (byte)RpcCalls.EnterVent,
        (byte)RpcCalls.ExitVent,
    ];

    public bool HandleAll(PlayerControl sender, MessageReader reader, 
        ref bool notify, ref string reason, ref bool ban)
    {
        return !sender.IsImpostor() && sender.GetRoleType() != RoleTypes.Engineer;
    }
}
