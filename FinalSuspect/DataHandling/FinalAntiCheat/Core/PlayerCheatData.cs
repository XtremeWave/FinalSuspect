using System;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using FinalSuspect.Patches.Game_Vanilla;
using InnerNet;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Core;

public class PlayerCheatData : IDisposable
{
    private readonly PlayerControl _player;

    private readonly Dictionary<byte, RpcRecord> _rpcRecords = new();

    public PlayerCheatData(PlayerControl player)
    {
        _player = player;
        ClientData = _player.GetClient();
    }

    public bool IsSuspectCheater { get; private set; }
    public bool IsHacker { get; private set; }
    public ClientData ClientData { get; private set; }
    public string FriendCode => ClientData?.FriendCode ?? string.Empty;
    public string Puid => ClientData?.GetHashedPuid() ?? string.Empty;
    public bool InComingOverloaded { get; private set; }

    public void Dispose()
    {
        IsSuspectCheater = false;
        ClientData = null;
        InComingOverloaded = false;
        _rpcRecords.Clear();
    }

    public void MarkAsCheater()
    {
        if (IsSuspectCheater) return;
        IsSuspectCheater = true;
        Warn($"Suspect Cheater: {_player.GetFinalData().Name}," +
             $"FriendCode: {FriendCode}," +
             $"Puid: {Puid},",
            "FAC");
    }

    public void MarkAsHacker()
    {
        if (IsHacker) return;
        IsHacker = true;
        Warn($"Overload Hacker: {_player.GetFinalData().Name}," +
             $"FriendCode: {FriendCode}," +
             $"Puid: {Puid},",
            "FAC");
    }

    private void HandleLobbyPosition()
    {
        if (!IsLobby) return;
        var pos = _player.GetTruePosition();
        if (pos.x > 3.5f || pos.x < -3.5f || pos.y > 4f || pos.y < -1f)
            MarkAsCheater();
    }

    private void HandleBan()
    {
        if (ClientData.IsFACPlayer() || ClientData.IsBannedPlayer())
            MarkAsCheater();
    }

    private void HandleSuspectCheater()
    {
        if (!Main.EnableFAC.Value || !IsSuspectCheater ||
            (_lastHandleCheater != -1 && _lastHandleCheater + 1 >= GetTimeStamp())) return;
        _lastHandleCheater = GetTimeStamp();
        if (!AmongUsClient.Instance.AmHost)
        {
            NotificationPopperPatch.NotificationPop(string.Format(GetString("CheatDetected.Cheater_NotHost"),
                _player.GetColoredName()));
            return;
        }

        KickPlayer(_player.PlayerId, false, "Cheater");
    }

    private void HandleHacker()
    {
        if (!Main.EnableGuardian.Value || !IsHacker ||
            (_lastHandleCheater != -1 && _lastHandleCheater + 1 >= GetTimeStamp())) return;
        _lastHandleCheater = GetTimeStamp();
        if (!AmongUsClient.Instance.AmHost)
        {
            NotificationPopperPatch.NotificationPop(string.Format(GetString("CheatDetected.Overload_NotHost"),
                _player.GetColoredName()));
            return;
        }

        KickPlayer(_player.PlayerId, false, "Overload");
    }

    public bool HandleIncomingRpc(byte rpcId)
    {
        if (InComingOverloaded) return true;
        var currentTime = GetCurrentTimestamp();

        if (_rpcRecords.TryGetValue(rpcId, out var record))
        {
            var timeDiff = currentTime - record.LastReceivedTime;

            if (timeDiff > 1000)
            {
                record.Count = 1;
                record.LastReceivedTime = currentTime;
            }
            else
            {
                record.Count++;

                if (record.Count > record.MaxiCount)
                {
                    MarkAsCheater();
                    record.Count = 0;
                    InComingOverloaded = true;
                    Warn($"InComingRpc Overloaded: {_player.GetDataName()}", "FAC");
                    return true;
                }
            }

            _rpcRecords[rpcId] = record;
        }
        else
        {
            _rpcRecords[rpcId] = new RpcRecord
            {
                LastReceivedTime = currentTime,
                Count = 1,
                MaxiCount = _handlers.Where(handlers => handlers.TargetRpcs.Contains(rpcId))
                    .SelectMany(handlers => handlers.Handlers)
                    .Where(handler => handler.Condition(_player))
                    .Select(handler => handler.MaxiReceivedNumPerSecond())
                    .Prepend(5)
                    .Max()
            };
        }

        return false;
    }

    public void HandleCheatData()
    {
        try
        {
            ClientData ??= _player.GetClient();
            HandleBan();
            HandleLobbyPosition();
            HandleSuspectCheater();
            HandleHacker();
        }
        catch
        {
            /* ignored */
        }
    }

    private struct RpcRecord
    {
        public long LastReceivedTime;
        public int Count;
        public int MaxiCount;
    }
}