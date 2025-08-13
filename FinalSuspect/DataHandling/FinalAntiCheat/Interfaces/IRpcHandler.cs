using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;

public interface IRpcHandler
{
    List<byte> TargetRpcs { get; }

    bool HandleInvalidRPC(PlayerControl sender, MessageReader reader, ref bool notify, ref string reason,
        ref bool ban)
    {
        return true;
    }

    bool HandleAll(PlayerControl sender, MessageReader reader, ref bool notify, ref string reason,
        ref bool ban)
    {
        return false;
    }

    bool HandleLobby(PlayerControl sender, MessageReader reader, ref bool notify, ref string reason,
        ref bool ban)
    {
        return false;
    }

    bool HandleGame_All(PlayerControl sender, MessageReader reader, ref bool notify, ref string reason,
        ref bool ban)
    {
        return false;
    }

    bool HandleGame_InTask(PlayerControl sender, MessageReader reader, ref bool notify, ref string reason,
        ref bool ban)
    {
        return false;
    }

    bool HandleGame_InMeeting(PlayerControl sender, MessageReader reader, ref bool notify, ref string reason,
        ref bool ban)
    {
        return false;
    }

    int MaxiReceivedNumPerSecond()
    {
        return 5;
    }

    bool Condition(PlayerControl player)
    {
        return true;
    }

    void Dispose(byte id)
    {
    }
}