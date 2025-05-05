using System.Collections.Generic;
using AmongUs.GameOptions;
using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using FinalSuspect.Modules.Core.Game;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Valid;

// 62~65
public class PhantomHandler : IRpcHandler
{
    public List<byte> TargetRpcs =>
    [
        (byte)RpcCalls.CheckVanish,
        (byte)RpcCalls.StartVanish,
        (byte)RpcCalls.CheckAppear,
        (byte)RpcCalls.StartAppear,
    ];
    
    public bool HandleLobby(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        return true;
    }
    
    public bool HandleGame_All(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        return sender.GetRoleType() is not RoleTypes.Phantom;
    }
}