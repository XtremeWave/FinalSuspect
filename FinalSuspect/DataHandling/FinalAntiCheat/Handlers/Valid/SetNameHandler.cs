using System.Collections.Generic;
using FinalSuspect.DataHandling.FinalAntiCheat.Interfaces;
using Hazel;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Handlers.Valid;

// 5, 6
public class SetNameHandler : IRpcHandler
{
    private static readonly Dictionary<byte, int> _counters = new();
    
    public List<byte> TargetRpcs =>
    [
        (byte)RpcCalls.CheckName,
        (byte)RpcCalls.SetName,
    ];

    public bool HandleAll(PlayerControl sender, MessageReader reader, 
        ref bool notify, ref string reason, ref bool ban)
    {
        _counters.TryAdd(sender.PlayerId, 0);
        if (++_counters[sender.PlayerId] <= 3) return false;
        if (AmongUsClient.Instance.AmHost)
        {
            HandleCheat(sender, GetString("Warning.SetName"));
            WarnHost();
        }
        else if (!OtherModHost)
        {
            HandleCheat(sender, GetString("Warning.SetName_NotHost"));
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
        _counters.Remove(id);
    }
}