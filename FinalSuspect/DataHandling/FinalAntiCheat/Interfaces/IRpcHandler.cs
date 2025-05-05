using System.Collections.Generic;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;

public interface IRpcHandler
{
    List<byte> TargetRpcs { get; }

    bool HandleInvalidRPC(PlayerControl sender, MessageReader reader, ref bool notify, ref string reason, ref bool ban) =>
        true;

    bool HandleAll(PlayerControl sender, MessageReader reader, ref bool notify, ref string reason, ref bool ban) =>
        false;

    bool HandleLobby(PlayerControl sender, MessageReader reader, ref bool notify, ref string reason, ref bool ban) =>
        false;

    bool HandleGame_All(PlayerControl sender, MessageReader reader, ref bool notify, ref string reason, ref bool ban) =>
        false;

    bool HandleGame_InTask(PlayerControl sender, MessageReader reader, ref bool notify, ref string reason,ref bool ban) =>
        false;

    bool HandleGame_InMeeting(PlayerControl sender, MessageReader reader, ref bool notify, ref string reason, ref bool ban) =>
        false;

    void Dispose(byte id) { }
}