using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using Hazel;
using InnerNet;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Valid;

// 12, 47
public class MurderPlayerHandler : IRpcHandler
{
    public List<byte> TargetRpcs =>
    [
        (byte)RpcCalls.MurderPlayer,
        (byte)RpcCalls.CheckMurder
    ];

    public bool HandleGame_InTask(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        var target = reader.ReadNetObject<PlayerControl>();
        var resultFlags = (MurderResultFlags)reader.ReadInt32();

        return !sender.IsImpostor()
               && sender != target
               && (resultFlags != MurderResultFlags.DecisionByHost || !sender.IsHost());
    }

    public bool HandleLobby(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        return true;
    }
}