//using SixLabors.ImageSharp.PixelFormats;
//using System.Drawing.Imaging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Resources;
using FinalSuspect.Patches.System;
using Il2CppInterop.Runtime.InteropTypes;
using InnerNet;
using UnityEngine;

namespace FinalSuspect.Modules.Core.Game;

public static class Utils
{
    private static readonly DateTime timeStampStartTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static long TimeStamp => (long)(DateTime.Now.ToUniversalTime() - timeStampStartTime).TotalSeconds;
    public static long GetTimeStamp(DateTime? dateTime = null) => (long)((dateTime ?? DateTime.Now).ToUniversalTime() - timeStampStartTime).TotalSeconds;
    public static ClientData GetClientById(int id)
    {
        try
        {
            var client = AmongUsClient.Instance.allClients.ToArray().Where(cd => cd.Id == id).FirstOrDefault();
            return client;
        }
        catch
        {
            return null;
        }
    }
    public static string GetRoleName(RoleTypes role, bool forUser = true)
    {
        return GetRoleString(Enum.GetName(typeof(RoleTypes), role), forUser);
    }
    public static Color GetRoleColor(RoleTypes role)
    {
        Main.roleColors.TryGetValue(role, out var hexColor);
        _ = ColorUtility.TryParseHtmlString(hexColor, out var c);
        return c;
    }
    public static string GetRoleColorCode(RoleTypes role)
    {
         Main.roleColors.TryGetValue(role, out var hexColor);
        return hexColor;
    }
    public static string GetRoleInfoForVanilla(this RoleTypes role, bool InfoLong = false)
    {
        if (role is RoleTypes.Crewmate or RoleTypes.Impostor)
            InfoLong = false;

        var text = role.ToString();

        var Info = "Blurb" + (InfoLong ? "Long" : "");

        if (!XtremeGameData.GameStates.IsNormalGame) text = "HnS" + text;

        return GetString($"{text}{Info}");
    }

    public static void KickPlayer(int clientId, bool ban, string reason = "")
    {
        XtremeLogger.Info($"try to kick {GetClientById(clientId)?.Character?.GetRealName()}", "Kick");
        AmongUsClient.Instance.KickPlayer(clientId, ban);
        OnPlayerLeftPatch.Add(clientId);
    }
    public static void KickPlayer(byte playerId, bool ban, string reason = "")
    {
        XtremeLogger.Info($"try to kick {GetPlayerById(playerId)?.GetRealName()}", "Kick");
        AmongUsClient.Instance.KickPlayer(playerId, ban);
        OnPlayerLeftPatch.Add(playerId);
    }
    public static string PadRightV2(this object text, int num)
    {
        var bc = 0;
        var t = text.ToString();
        foreach (var c in t) bc += Encoding.GetEncoding("UTF-8").GetByteCount(c.ToString()) == 1 ? 1 : 2;
        return t?.PadRight(Mathf.Max(num - (bc - t.Length), 0));
    }
    public static DirectoryInfo GetLogFolder(bool auto = false)
    {
        var folder = Directory.CreateDirectory($"{Application.persistentDataPath}/FinalSuspect/Logs");
        if (auto)
        {
            folder = Directory.CreateDirectory($"{folder.FullName}/AutoLogs");
        }
        return folder;
    }
    public static void DumpLog(bool popup = false)
    {
        var logs = GetLogFolder();
        var filename = CopyLog(logs.FullName);
        OpenDirectory(filename);
        if (PlayerControl.LocalPlayer != null)
        {
            var t = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
            if (popup) 
                HudManager.Instance.ShowPopUp(string.Format(GetString("Message.DumpfileSaved"), $"FinalSuspect - v{Main.DisplayedVersion}-{t}.log"));
            else AddChatMessage(string.Format(GetString("Message.DumpfileSaved"), $"FinalSuspect - v{Main.DisplayedVersion}-{t}.log"));
        }
    }
    public static void SaveNowLog()
    {
        var logs = GetLogFolder(true);
        logs.EnumerateFiles().Where(f => f.CreationTime < DateTime.Now.AddDays(-7)).ToList().ForEach(f => f.Delete());
        CopyLog(logs.FullName);
    }
    public static string CopyLog(string path)
    {
        var f = $"{path}/Final Suspect-logs/";
        var t = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
        var fileName = $"{f}FinalSuspect-v{Main.DisplayedVersion}-{t}.log";
        if (!Directory.Exists(f)) Directory.CreateDirectory(f);
        FileInfo file = new(@$"{Environment.CurrentDirectory}/BepInEx/LogOutput.log");
        var logFile = file.CopyTo(fileName);
        return logFile.FullName;
    }
    public static void OpenDirectory(string path)
    {
        Process.Start("Explorer.exe", $"/select,{path}");
    }
    public static string SummaryTexts(byte id)
    {

        var thisdata = XtremePlayerData.GetXtremeDataById(id);

        var builder = new StringBuilder();
        var longestNameByteCount = XtremePlayerData.GetLongestNameByteCount();


        var pos = Math.Min(((float)longestNameByteCount / 2) + 1.5f, 11.5f);


        var colorId = thisdata.ColorId;
        builder.Append(StringHelper.ColorString(Palette.PlayerColors[colorId], thisdata.Name));
        pos += 1.5f;
        builder.AppendFormat("<pos={0}em>", pos).Append(GetProgressText(id)).Append("</pos>");
        pos += 4.5f;
        
        builder.AppendFormat("<pos={0}em>", pos).Append(GetVitalText(id, true)).Append("</pos>");
        pos += DestroyableSingleton<TranslationController>.Instance.currentLanguage.languageID == SupportedLangs.English ? 14f : 10.5f;

        builder.AppendFormat("<pos={0}em>", pos);

        var oldrole = thisdata.RoleWhenAlive ?? RoleTypes.Crewmate;
        var newrole = thisdata.RoleAfterDeath ?? (thisdata.IsImpostor? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost);
        builder.Append(StringHelper.ColorString(GetRoleColor(oldrole), GetString($"{oldrole}")));

        if (thisdata.IsDead  && newrole != oldrole)
        {
            builder.Append($"=> {StringHelper.ColorString(GetRoleColor(newrole), GetRoleString($"{newrole}"))}");
        }
        builder.Append("</pos>");

        return builder.ToString();
    }

    public static Dictionary<string, Sprite> CachedSprites = new();

    public static Sprite LoadSprite(string file, float pixelsPerUnit = 1f)
    {
        try
        {
            if (CachedSprites.TryGetValue(file + pixelsPerUnit, out var sprite)) return sprite;
            var texture = LoadTextureFromResources(file);
            sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return CachedSprites[file + pixelsPerUnit] = sprite;
        }
        catch
        {
            XtremeLogger.Error($"读入Texture失败：{file}", "LoadImage");
        }
        return null;
    }

    public static Texture2D LoadTextureFromResources(string file)
    {
        var path = PathManager.GetResourceFilesPath(FileType.Images, file);

        try
        {
            if (!File.Exists(path))
                goto InDLL;
            
            var fileData = File.ReadAllBytes(path);
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            if (texture.LoadImage(fileData))
            {
                return texture;
            }

            XtremeLogger.Warn($"无法读取图片：{path}", "LoadTexture");
        }
        catch (Exception ex)
        {
            XtremeLogger.Warn($"读入Texture失败：{path} - {ex.Message}", "LoadTexture");
        }
        InDLL:
        /*path = "FinalSuspect.Resources.Images." + file;

        try
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            using MemoryStream ms = new();
            stream.CopyTo(ms);
            texture.LoadImage(ms.ToArray(), false);
            return texture;
        }
        catch
        {
            XtremeLogger.Error($"读入Texture失败：{path}", "LoadImage");
        }*/
        return null;
    }

    /// <summary>
    /// 乱数の簡易的なヒストグラムを取得する関数
    /// <params name="nums">生成した乱数を格納したint配列</params>
    /// <params name="scale">ヒストグラムの倍率 大量の乱数を扱う場合、この値を下げることをお勧めします。</params>
    /// </summary>

    public static bool TryCast<T>(this Il2CppObjectBase obj, out T casted)
    where T : Il2CppObjectBase
    {
        casted = obj.TryCast<T>();
        return casted != null;
    }

    private const string ActiveSettingsSize = "70%";
    private const string ActiveSettingsLineHeight = "55%";

    public static bool AmDev() => IsDev(EOSManager.Instance.FriendCode);
    public static bool IsDev(this PlayerControl pc) => IsDev(pc.FriendCode);
    public static bool IsDev(string friendCode) => friendCode
        is "teamelder#5856" //Slok
        ;
    public static void AddChatMessage(string text, string title = "")
    {
        if (!AmongUsClient.Instance.AmHost) return;
        var player = PlayerControl.LocalPlayer;
        var name = player.Data.PlayerName;
        player.SetName(title + '\0');
        DestroyableSingleton<HudManager>.Instance?.Chat?.AddChat(player, text);
        player.SetName(name);
    }

    private static Dictionary<byte, PlayerControl> cachedPlayers = new();

    public static PlayerControl GetPlayerById(int playerId) => GetPlayerById((byte)playerId);
    public static PlayerControl GetPlayerById(byte playerId)
    {
        if (cachedPlayers.TryGetValue(playerId, out var cachedPlayer) && cachedPlayer != null)
        {
            return cachedPlayer;
        }
        var player = Main.AllPlayerControls.Where(pc => pc.PlayerId == playerId).FirstOrDefault();
        cachedPlayers[playerId] = player;
        return player;
    }


    public static string GetProgressText(PlayerControl pc = null)
    {
        pc ??= PlayerControl.LocalPlayer;

        var enable = CanSeeTargetRole(pc, out var bothImp) || bothImp;


        var comms = IsActive(SystemTypes.Comms);
        var text = GetProgressText(pc.PlayerId, comms);
        return enable? text : "";
    }
    private static string GetProgressText(byte playerId, bool comms = false)
    {
        return GetTaskProgressText(playerId, comms);
    }
    public static string GetTaskProgressText(byte playerId, bool comms = false)
    {
        var data = XtremePlayerData.GetXtremeDataById(playerId);
        if (!XtremeGameData.GameStates.IsNormalGame)
        {
            if (data.IsImpostor)
            {
                var KillColor = Palette.ImpostorRed;
                return StringHelper.ColorString(KillColor, $"({GetString("KillCount")}: {data.KillCount})");
            }
            return "";
        }

        
        if (data.IsImpostor)
        {
            var KillColor = data.IsDisconnected ? Color.gray : Palette.ImpostorRed;
            return StringHelper.ColorString(KillColor, $"({GetString("KillCount")}: {data.KillCount})");
        }
        var NormalColor = data.TaskCompleted ? Color.green : Color.yellow;
        var TextColor = comms || data.IsDisconnected ? Color.gray : NormalColor;
        var Completed = comms ? "?" : $"{data.CompleteTaskCount}";
        return StringHelper.ColorString(TextColor, $"({Completed}/{data.TotalTaskCount})");

    }
    public static string GetVitalText(byte playerId, bool summary = false, bool docolor = true)
    {
        var data = XtremePlayerData.GetXtremeDataById(playerId);
        if (!data.IsDead || data.RealDeathReason is VanillaDeathReason.None) return "";
        
        var deathReason = GetString("DeathReason." + data.RealDeathReason);
        var color = Palette.CrewmateBlue;
        switch (data.RealDeathReason)
        {
            case VanillaDeathReason.Disconnect:
                color = Color.gray;
                break;
            case VanillaDeathReason.Kill:
                color = Palette.ImpostorRed;
                var killercolor = Palette.PlayerColors[data.RealKiller.ColorId];

                if (summary)
                    deathReason += $"<=<size=80%>{StringHelper.ColorString(killercolor, data.RealKiller.Name)}</size>";
                else if (docolor)
                    deathReason = StringHelper.ColorString(killercolor, deathReason);
                break;
            case VanillaDeathReason.Exile:
                color = Palette.Purple;
                break;
        }

        if (!summary) deathReason = "(" + deathReason + ")";
        
        deathReason = StringHelper.ColorString(color, deathReason) ;

        return deathReason;
    }
    public static bool IsActive(SystemTypes type)
    {
        if (!XtremeGameData.GameStates.IsNormalGame) return false;
        if (!ShipStatus.Instance.Systems.ContainsKey(type))
        {
            return false;
        }
        int mapId = Main.NormalOptions.MapId;
        switch (type)
        {
            case SystemTypes.Electrical:
                {
                    var SwitchSystem = ShipStatus.Instance.Systems[type].Cast<SwitchSystem>();
                    return SwitchSystem != null && SwitchSystem.IsActive;
                }
            case SystemTypes.Reactor:
            {
                if (mapId == 2) return false;
                var ReactorSystemType = ShipStatus.Instance.Systems[type].Cast<ReactorSystemType>();
                return ReactorSystemType != null && ReactorSystemType.IsActive;
            }
            case SystemTypes.Laboratory:
                {
                    if (mapId != 2) return false;
                    var ReactorSystemType = ShipStatus.Instance.Systems[type].Cast<ReactorSystemType>();
                    return ReactorSystemType != null && ReactorSystemType.IsActive;
                }
            case SystemTypes.LifeSupp:
                {
                    if (mapId is 2 or 4) return false;
                    var LifeSuppSystemType = ShipStatus.Instance.Systems[type].Cast<LifeSuppSystemType>();
                    return LifeSuppSystemType != null && LifeSuppSystemType.IsActive;
                }
            case SystemTypes.Comms:
            {
                if (mapId is 1 or 5)
                {
                    var HqHudSystemType = ShipStatus.Instance.Systems[type].Cast<HqHudSystemType>();
                    return HqHudSystemType != null && HqHudSystemType.IsActive;
                }

                var HudOverrideSystemType = ShipStatus.Instance.Systems[type].Cast<HudOverrideSystemType>();
                return HudOverrideSystemType != null && HudOverrideSystemType.IsActive;
            }
            case SystemTypes.HeliSabotage:
                {
                    var HeliSabotageSystem = ShipStatus.Instance.Systems[type].Cast<HeliSabotageSystem>();
                    return HeliSabotageSystem != null && HeliSabotageSystem.IsActive;
                }
            case SystemTypes.MushroomMixupSabotage:
                {
                    var mushroomMixupSabotageSystem = ShipStatus.Instance.Systems[type].TryCast<MushroomMixupSabotageSystem>();
                    return mushroomMixupSabotageSystem != null && mushroomMixupSabotageSystem.IsActive;
                }
            default:
                return false;
        }
    }
    public static bool IsImpostor(RoleTypes role)
    {
        switch (role)
        {
            case RoleTypes.Impostor:
            case RoleTypes.Shapeshifter:
            case RoleTypes.Phantom:
            case RoleTypes.ImpostorGhost:
                return true;
            default:
                return false;
        }
    }
    public static bool CanSeeTargetRole(PlayerControl target, out bool bothImp)
    {
        var LocalDead = !PlayerControl.LocalPlayer.IsAlive();
        var IsAngel = PlayerControl.LocalPlayer.GetRoleType() is RoleTypes.GuardianAngel;
        var BothDeathCanSee = LocalDead && ((!target.IsAlive() && IsAngel) || !IsAngel);
        bothImp = PlayerControl.LocalPlayer.IsImpostor() && target.IsImpostor();


        return target.IsLocalPlayer() ||
        BothDeathCanSee ||
        bothImp && LocalDead;
    }
    public static bool CanSeeOthersRole()
    {
        if (!XtremeGameData.GameStates.IsInGame) return true;
        if (XtremeGameData.GameStates.IsFreePlay) return true;
        var LocalDead = !PlayerControl.LocalPlayer.IsAlive();
        var IsAngel = PlayerControl.LocalPlayer.GetRoleType() is RoleTypes.GuardianAngel;
        
        return !IsAngel && LocalDead;
    }
    public static void ExecuteWithTryCatch(this Action action, bool Log = false)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            if (Log)
            XtremeLogger.Error(ex.ToString(), "Execute With Try Catch");
        }
    }
}