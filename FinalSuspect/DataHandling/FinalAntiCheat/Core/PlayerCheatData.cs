using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using FinalSuspect.Patches.Game_Vanilla;
using InnerNet;

namespace FinalSuspect.DataHandling.FinalAntiCheat.Core;

public class PlayerCheatData
{
    public bool IsSuspectCheater { get; private set; }
    public ClientData ClientData { get; }
    public string FriendCode => ClientData.FriendCode;
    public string Puid => ClientData.GetHashedPuid();
    
    private readonly PlayerControl _player;

    public PlayerCheatData(PlayerControl player)
    {
        _player = player;
        ClientData = _player.GetClient();
    }

    public void MarkAsCheater() => IsSuspectCheater = true;

    public void HandleLobbyPosition()
    {
        if (!IsLobby) return;
        var pos = _player.GetTruePosition();
        if (pos.x > 3.5f || pos.x < -3.5f || pos.y > 4f || pos.y < -1f)
            MarkAsCheater();
    }

    public void HandleBan()
    {
        if (ClientData.IsFACPlayer() || ClientData.IsBannedPlayer())
            MarkAsCheater();
    }
    
    public void HandleSuspectCheater()
    {
        if (Main.DisableFAC.Value || !IsSuspectCheater || _lastHandleCheater != -1 && _lastHandleCheater + 1 >= GetTimeStamp()) return;
        _lastHandleCheater = GetTimeStamp();
        if (!AmongUsClient.Instance.AmHost)
        {
            NotificationPopperPatch.NotificationPop(string.Format(GetString("Warning.Cheater_NotHost"),
                _player.GetDataName()));
            return;
        }
        
        NotificationPopperPatch.NotificationPop(string.Format(GetString("Warning.Cheater"),
            _player.GetDataName()));
        KickPlayer(_player.PlayerId, false, "Suspect Cheater");
    }
    
    private static readonly Regex ValidFormatRegex = new(
        @"^[A-Za-z]+#\d{4}$", 
        RegexOptions.Compiled
    );

    private void HandleFriendCode()
    {
        if (!ValidFormatRegex.IsMatch(FriendCode) && Main.KickPlayerWhoFriendCodeNotExist.Value)
            MarkAsCheater();
    }
    
    private readonly Dictionary<byte, RpcRecord> _rpcRecords = new();
    
    private struct RpcRecord
    {
        public long LastReceivedTime; 
        public int Count;
    }
    
    public bool HandleIncomingRpc(byte rpcId)
    {
        var currentTime = GetCurrentTimestamp();
        
        if (_rpcRecords.TryGetValue(rpcId, out var record))
        {
            var timeDiff = currentTime - record.LastReceivedTime;
            
            if (timeDiff > 1000)
            {
                record.Count = 1;
            }
            else
            {
                record.Count++;
                
                if (record.Count > 10)
                {
                    MarkAsCheater();
                    record.Count = 0;
                    return true;
                }
            }
            record.LastReceivedTime = currentTime;
            _rpcRecords[rpcId] = record;
        }
        else
        {
            _rpcRecords[rpcId] = new RpcRecord
            {
                LastReceivedTime = currentTime,
                Count = 1
            };
        }
        return false;
    }
    
    private static long GetCurrentTimestamp()
    {
        return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }

    public void HandleCheatData()
    {
        try
        {
            HandleBan();
            HandleLobbyPosition();
            HandleSuspectCheater();
        }
        catch 
        {
            /* ignored */
        }
    }
}