using System;
using System.IO;
using System.Text.RegularExpressions;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using InnerNet;

namespace FinalSuspect.Modules.Features.CheckingandBlocking;

public static class BanManager
{
    public static readonly List<string> FACList = [];

    private static readonly Regex ValidFormatRegex = new(
        @"^[a-z]{7,10}#\d{4}$",
        RegexOptions.Compiled
    );

    public static void AddBanPlayer(ClientData player)
    {
        if (!AmongUsClient.Instance.AmHost || player == null) return;
        if (!player.IsBannedPlayer())
        {
            File.AppendAllText(BAN_LIST_PATH, $"{player.FriendCode},{player.GetHashedPuid()},{player.PlayerName}\n");
            SendInGame(string.Format(GetString("Notification.AddedPlayerToBanList"),
                StringHelper.ColorString(Palette.PlayerColors[player.ColorId], player.PlayerName)));
        }
        else
        {
            Info($"{player.FriendCode},{player.GetHashedPuid()},{player.PlayerName} 已经被加入封禁名单", "AddBanPlayer");
        }
    }

    public static void CheckDenyNamePlayer(ClientData player)
    {
        if (!AmongUsClient.Instance.AmHost || !ConfigManager.KickPlayerWithDenyName.Value) return;
        try
        {
            var existingNames = SpamManager.ReturnAllNewLinesInFile(DENY_NAME_LIST_PATH);

            var hasValidMatch = existingNames
                .Where(name => !IsDevFriendCode(name))
                .Any(IsNameMatch);

            if (!hasValidMatch) return;
            KickPlayer(player.Id, false, "KickedByDenyName");
            return;

            bool IsNameMatch(string pattern) =>
                !string.IsNullOrEmpty(pattern) && player.PlayerName.ToLower().Contains(pattern.ToLower());

            bool IsDevFriendCode(string name) =>
                Main.AllPlayerControls.Any(p => p.IsDev() && name.Contains(p.FriendCode));
        }
        catch (Exception ex)
        {
            Exception(ex, "CheckDenyNamePlayer");
        }
    }

    public static void CheckFriendCode(ClientData player)
    {
        if (!AmongUsClient.Instance.AmHost || !ConfigManager.KickPlayerWithAbnormalFriendCode.Value) return;
        //用于检测是否为xxx#1145/xxx#1337的重复代码前缀
        //InnerSloth的好友代码不会出现前端重复 如果有前端重复一定是UE或者SM黑客
        var currentPrefixes = AmongUsClient.Instance.allClients
            .ToArray()
            .Where(c => c.Id != player.Id)
            .Select(c =>
            {
                if (string.IsNullOrEmpty(c.FriendCode)) return null;
                var parts = c.FriendCode.Split('#');
                return parts.Length > 0 ? parts[0].ToLowerInvariant() : null;
            })
            .Where(prefix => !string.IsNullOrEmpty(prefix))
            .ToHashSet();
        var newPrefixParts = player.FriendCode.Split('#');
        var newPrefix = newPrefixParts[0].ToLowerInvariant();

        if (currentPrefixes.Contains(newPrefix))
        {
            KickPlayer(player.Id, false, "KickedByAbnormalFriendCode");
            Info($"重复好友代码前缀的玩家 {player.PlayerName} 已被踢出", "Kick");
        }
        else if (player.FriendCode == "")
        {
            KickPlayer(player.Id, false, "KickedByAbnormalFriendCode");
            Info($"没有好友代码的玩家 {player.PlayerName} 已被踢出。", "Kick");
        }
        else if (!ValidFormatRegex.IsMatch(player.FriendCode) || newPrefixParts.Length < 1)
        {
            KickPlayer(player.Id, false, "KickedByAbnormalFriendCode");
            Info($"好友代码格式异常玩家 {player.PlayerName} 已被踢出。", "Kick");
        }
    }

    public static void CheckBanPlayer(ClientData player)
    {
        if (!AmongUsClient.Instance.AmHost && !ConfigManager.KickPlayerInBanList.Value) return;
        if (player.IsBannedPlayer() ||
            DestroyableSingleton<FriendsListManager>.Instance.IsPlayerBlockedUsername(player.FriendCode))
            KickPlayer(player.Id, true, "BanedByBanList", KickLevel.CheatDetected);
        else if (player.IsFACPlayer()) KickPlayer(player.Id, true, "BanedByFACList", KickLevel.CheatDetected);
    }

    public static bool CheckBanStatus(string friendCode, string hashedPuid)
    {
        try
        {
            if (!File.Exists(BAN_LIST_PATH)) File.Create(BAN_LIST_PATH).Close();
            using StreamReader sr = new(BAN_LIST_PATH);
            while (sr.ReadLine() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (!string.IsNullOrWhiteSpace(friendCode) && line.Contains(friendCode)) return true;
                if (!string.IsNullOrWhiteSpace(hashedPuid) && line.Contains(hashedPuid)) return true;
            }
        }
        catch (Exception ex)
        {
            Exception(ex, "CheckBanList");
        }

        return false;
    }

    public static bool IsFACPlayer(this ClientData player)
    {
        return CheckFACStatus(player?.FriendCode, player?.GetHashedPuid());
    }

    public static bool CheckFACStatus(string friendCode, string hashedPuid)
    {
        return FACList.Any(line =>
            !string.IsNullOrWhiteSpace(friendCode) && line.Contains(friendCode) ||
            !string.IsNullOrWhiteSpace(hashedPuid) && line.Contains(hashedPuid));
    }
}

[HarmonyPatch(typeof(BanMenu), nameof(BanMenu.Select))]
internal class BanMenuSelectPatch
{
    public static void Postfix(BanMenu __instance, int clientId)
    {
        var recentClient = AmongUsClient.Instance.GetRecentClient(clientId);
        if (recentClient == null) return;
        if (recentClient.IsBannedPlayer())
            __instance.BanButton.GetComponent<ButtonRolloverHandler>().SetEnabledColors();
    }
}