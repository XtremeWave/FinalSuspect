using FinalSuspect.DataHandling.FinalAntiCheat.Core;
using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Valid;

// 5, 6
public class SetNameHandler : IRpcHandler
{
    private static readonly Dictionary<byte, int> Counters = new();

    public List<byte> TargetRpcs =>
    [
        (byte)RpcCalls.CheckName,
        (byte)RpcCalls.SetName
    ];

    public bool HandleAll(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        Counters.TryAdd(sender.PlayerId, 0);
        if (++Counters[sender.PlayerId] <= 3) return false;
        if (AmHost)
        {
            HandleCheat(sender, GetString(CheatDetected.SetName));
            WarnHost();
        }
        else if (!OtherModHost)
        {
            HandleCheat(sender, GetString(CheatDetected.SetName_NotHost));
        }

        ban = true;
        notify = false;
        return true;
    }

    public bool HandleGame_All(PlayerControl sender, MessageReader reader,
        ref bool notify, ref string reason, ref bool ban)
    {
        return true;
    }

    public void Dispose(byte id)
    {
        Counters.Remove(id);
    }
}