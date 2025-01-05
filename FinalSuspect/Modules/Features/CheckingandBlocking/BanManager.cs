using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using FinalSuspect.Attributes;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Modules.Resources;
using FinalSuspect.Patches.Game_Vanilla;
using HarmonyLib;
using InnerNet;
using static FinalSuspect.Modules.Core.Plugin.Translator;

namespace FinalSuspect.Modules.Features.CheckingandBlocking;

public static class BanManager
{
    
    private static readonly string BAN_LIST_PATH = PathManager.GetBanFilesPath("BanList.txt");
    private static List<string> FACList = new();

    [PluginModuleInitializer]
    public static void Init()
    {
        try
        {
            if (!File.Exists(BAN_LIST_PATH))
            {
                Core.Plugin.Logger.Warn("Create New BanList.txt", "BanManager");
                File.Create(BAN_LIST_PATH).Close();
            }

            //读取FAC名单
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FinalSuspect.Resources.Configs.FACList.txt");
            stream.Position = 0;
            using StreamReader sr = new(stream, Encoding.UTF8);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                if (Main.AllPlayerControls.Any(p => p.IsDev() && line.Contains(p.FriendCode))) continue;
                FACList.Add(line);
            }
        }
        catch (Exception ex)
        {
            Core.Plugin.Logger.Exception(ex, "BanManager");
        }
    }
    private static string GetResourcesTxt(string path)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        stream.Position = 0;
        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    public static string GetHashedPuid(this PlayerControl player)
        => player.GetClient().GetHashedPuid();
    public static string GetHashedPuid(this ClientData player)
    {
        if (player == null) return null;
        string puid = player.ProductUserId;
        if (string.IsNullOrEmpty(puid)) return puid;

        using SHA256 sha256 = SHA256.Create();
        string sha256Hash = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(puid))).Replace("-", "").ToLower();
        return string.Concat(sha256Hash.AsSpan(0, 5), sha256Hash.AsSpan(sha256Hash.Length - 4));
    }
    public static void AddBanPlayer(ClientData player)
    {
        if (!AmongUsClient.Instance.AmHost || player == null) return;
        if (player.IsBannedPlayer())
        {
            File.AppendAllText(BAN_LIST_PATH, $"{player.FriendCode},{player.GetHashedPuid()},{player.PlayerName}\n");
            Core.Plugin.Logger.SendInGame(string.Format(GetString("Message.AddedPlayerToBanList"), player.PlayerName));
        }
    }
    public static void CheckDenyNamePlayer(ClientData player)
    {
        if (!AmongUsClient.Instance.AmHost || !Main.KickPlayerWithDenyName.Value) return;
        try
        {
            using StreamReader sr = new(SpamManager.DENY_NAME_LIST_PATH);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line == "") continue;
                if (Main.AllPlayerControls.Any(p => p.IsDev() && line.Contains(p.FriendCode))) continue;
                if (Regex.IsMatch(player.PlayerName, line))
                {
                    Utils.KickPlayer(player.Id, false, "DenyName");
                    NotificationPopperPatch.NotificationPop(string.Format(GetString("Message.KickedByDenyName"), player.PlayerName, line));
                    Core.Plugin.Logger.Info($"{player.PlayerName}は名前が「{line}」に一致したためキックされました。", "Kick");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Core.Plugin.Logger.Exception(ex, "CheckDenyNamePlayer");
        }
    }

    public static void CheckBanPlayer(ClientData player)
    {
        if (!AmongUsClient.Instance.AmHost || !Main.KickPlayerInBanList.Value) return;
        if (player.IsBannedPlayer())
        {
            Utils.KickPlayer(player.Id, true, "BanList");
            NotificationPopperPatch.NotificationPop(string.Format(GetString("Message.BanedByBanList"), player.PlayerName));
            Core.Plugin.Logger.Info($"{player.PlayerName}は過去にBAN済みのためBANされました。", "BAN");
        }
        else if (player.IsFACPlayer())
        {
            Utils.KickPlayer(player.Id, true, "FACList");
            NotificationPopperPatch.NotificationPop(string.Format(GetString("Message.BanedByFACList"), player.PlayerName));
            Core.Plugin.Logger.Info($"{player.PlayerName}存在于FAC封禁名单", "BAN");
        }
    }

    public static bool IsBannedPlayer(this PlayerControl player)
        => player?.GetClient()?.IsBannedPlayer() ?? false;
    public static bool IsBannedPlayer(this ClientData player)
        => CheckBanStatus(player?.FriendCode, player?.GetHashedPuid());
    public static bool CheckBanStatus(string friendCode, string hashedPuid)
    {
        try
        {
            Directory.CreateDirectory("Final Suspect_Data");
            if (!File.Exists(BAN_LIST_PATH)) File.Create(BAN_LIST_PATH).Close();
            using StreamReader sr = new(BAN_LIST_PATH);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (!string.IsNullOrWhiteSpace(friendCode) && line.Contains(friendCode)) return true;
                if (!string.IsNullOrWhiteSpace(hashedPuid) && line.Contains(hashedPuid)) return true;
            }
        }
        catch (Exception ex)
        {
            Core.Plugin.Logger.Exception(ex, "CheckBanList");
        }
        return false;
    }

    public static bool IsFACPlayer(this PlayerControl player)
        => player?.GetClient()?.IsFACPlayer() ?? false;
    public static bool IsFACPlayer(this ClientData player)
        => CheckFACStatus(player?.FriendCode, player?.GetHashedPuid());
    public static bool CheckFACStatus(string friendCode, string hashedPuid)
        => FACList.Any(line =>
        !string.IsNullOrWhiteSpace(friendCode) && line.Contains(friendCode) ||
        !string.IsNullOrWhiteSpace(hashedPuid) && line.Contains(hashedPuid));
}
[HarmonyPatch(typeof(BanMenu), nameof(BanMenu.Select))]
class BanMenuSelectPatch
{
    public static void Postfix(BanMenu __instance, int clientId)
    {
        ClientData recentClient = AmongUsClient.Instance.GetRecentClient(clientId);
        if (recentClient == null) return;
        if (recentClient.IsBannedPlayer())
            __instance.BanButton.GetComponent<ButtonRolloverHandler>().SetEnabledColors();
    }
}
[HarmonyPatch(typeof(BanMenu), nameof(BanMenu.Update))]
class BanMenuUpdatePatch
{
    public static void Postfix(BanMenu __instance)
    {
        __instance.KickButton.gameObject.SetActive(true);
        __instance.BanButton.gameObject.SetActive(true);
    }
}