using System;
using System.Security.Cryptography;
using System.Text;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using InnerNet;

namespace FinalSuspect.Modules.Core.Game.PlayerControlExtension;

public static class _Cheat
{
    public static bool IsBannedPlayer(this ClientData player)
    {
        return BanManager.CheckBanStatus(player?.FriendCode, player?.GetHashedPuid());
    }

    public static string GetHashedPuid(this ClientData player)
    {
        if (player == null) return null;
        var puid = player.ProductUserId;
        if (string.IsNullOrEmpty(puid)) return puid;

        using var sha256 = SHA256.Create();
        var sha256Hash = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(puid))).Replace("-", "")
            .ToLower();
        return string.Concat(sha256Hash.AsSpan(0, 5), sha256Hash.AsSpan(sha256Hash.Length - 4));
    }

    extension(PlayerControl player)
    {
        public bool IsFACPlayer()
        {
            return player?.GetClient()?.IsFACPlayer() ?? false;
        }

        public bool IsBannedPlayer()
        {
            return player?.GetClient()?.IsBannedPlayer() ?? false;
        }
    }

    extension(PlayerControl pc)
    {
        public void MarkAsCheater()
        {
            pc.GetData().CheatData.MarkAsCheater();
        }

        public void MarkAsHacker()
        {
            pc.GetData().CheatData.MarkAsHacker();
        }

        public string GetHashedPuid()
        {
            return pc.GetClient().GetHashedPuid();
        }
    }
}