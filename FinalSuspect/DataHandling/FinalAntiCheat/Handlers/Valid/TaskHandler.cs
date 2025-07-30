using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Valid;

// 1
public class TaskHandler : IRpcHandler
{
    public List<byte> TargetRpcs =>
    [
        (byte)RpcCalls.CompleteTask
    ];

    public bool HandleLobby(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        return true;
    }

    public bool HandleGame_InTask(PlayerControl sender, MessageReader reader, ref bool notify, ref string reason,
        ref bool ban)
    {
        return sender.IsImpostor();
    }

    public bool HandleGame_InMeeting(PlayerControl sender, MessageReader reader, ref bool notify, ref string reason,
        ref bool ban)
    {
        return true;
    }
}