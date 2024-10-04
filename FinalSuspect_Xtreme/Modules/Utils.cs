using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes;
using InnerNet;
//using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
//using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using static FinalSuspect_Xtreme.Translator;

namespace FinalSuspect_Xtreme;

public static class Utils
{
    private static readonly DateTime timeStampStartTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static long TimeStamp => (long)(DateTime.Now.ToUniversalTime() - timeStampStartTime).TotalSeconds;
    public static long GetTimeStamp(DateTime? dateTime = null) => (long)((dateTime ?? DateTime.Now).ToUniversalTime() - timeStampStartTime).TotalSeconds;

    public static Vector2 LocalPlayerLastTp;
    public static bool LocationLocked = false;
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
        _ = ColorUtility.TryParseHtmlString(hexColor, out Color c);
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

    public static void KickPlayer(int playerId, bool ban, string reason)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        AmongUsClient.Instance.KickPlayer(playerId, ban);
        OnPlayerLeftPatch.Add(playerId);

    }
    public static string PadRightV2(this object text, int num)
    {
        int bc = 0;
        var t = text.ToString();
        foreach (char c in t) bc += Encoding.GetEncoding("UTF-8").GetByteCount(c.ToString()) == 1 ? 1 : 2;
        return t?.PadRight(Mathf.Max(num - (bc - t.Length), 0));
    }
    public static void DumpLog(bool popup = false)
    {
        string f = $"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}/FinalSuspect_Xtreme-logs/";
        string t = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
        string filename = $"{f}FinalSuspect_Xtreme-v{Main.DisplayedVersion}-{t}.log";
        if (!Directory.Exists(f)) Directory.CreateDirectory(f);
        FileInfo file = new(@$"{Environment.CurrentDirectory}/BepInEx/LogOutput.log");
        file.CopyTo(@filename);
        if (PlayerControl.LocalPlayer != null)
        {
            if (popup) //PlayerControl.LocalPlayer.ShowPopUp(string.Format(GetString("Message.DumpfileSaved"), $"FinalSuspect_Xtreme - v{Main.DisplayedVersion}-{t}.log"));
                HudManager.Instance.ShowPopUp(string.Format(GetString("Message.DumpfileSaved"), $"FinalSuspect_Xtreme - v{Main.DisplayedVersion}-{t}.log"));
            else AddChatMessage(string.Format(GetString("Message.DumpfileSaved"), $"FinalSuspect_Xtreme - v{Main.DisplayedVersion}-{t}.log"));
        }
        ProcessStartInfo psi = new ProcessStartInfo("Explorer.exe")
        { Arguments = "/e,/select," + @filename.Replace("/", "\\") };
        Process.Start(psi);
    }
    public static void OpenDirectory(string path)
    {
        var startInfo = new ProcessStartInfo(path)
        {
            UseShellExecute = true,
        };
        Process.Start(startInfo);
    }
    public static string SummaryTexts(byte id)
    {

        var thisdata = XtremeGameData.XtremePlayerData.GetPlayerDataById(id);

        var builder = new StringBuilder();
        var longestNameByteCount = XtremeGameData.XtremePlayerData.GetLongestNameByteCount();


        var pos = Math.Min(((float)longestNameByteCount / 2) + 1.5f, 11.5f);


        var colorId = thisdata.PlayerColor;
        builder.Append(ColorString(Palette.PlayerColors[colorId], thisdata.PlayerName));

        builder.AppendFormat("<pos={0}em>", pos).Append(GetProgressText(id)).Append("</pos>");
        pos += 4.5f;
        
        builder.AppendFormat("<pos={0}em>", pos).Append(GetVitalText(id, true)).Append("</pos>");
        pos += DestroyableSingleton<TranslationController>.Instance.currentLanguage.languageID == SupportedLangs.English ? 14f : 10.5f;

        builder.AppendFormat("<pos={0}em>", pos);

        var oldrole = thisdata.RoleWhenAlive ?? RoleTypes.Crewmate;
        var newrole = thisdata.RoleAfterDeath ?? (thisdata.IsImpostor? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost);
        builder.Append(ColorString(GetRoleColor(oldrole), GetString($"{oldrole}")));

        if (thisdata.IsDead  && newrole != oldrole)
        {
            builder.Append($"=> {ColorString(GetRoleColor(newrole), GetRoleString($"{newrole}"))}");
        }
        builder.Append("</pos>");

        return builder.ToString();
    }

    public static string RemoveHtmlTags(this string str) => Regex.Replace(str, "<[^>]*?>", string.Empty);
    public static string RemoveHtmlTagsExcept(this string str, string exceptionLabel) => Regex.Replace(str, "<(?!/*" + exceptionLabel + ")[^>]*?>", string.Empty);
    public static string RemoveColorTags(this string str) => Regex.Replace(str, "</?color(=#[0-9a-fA-F]*)?>", "");
    
    public static Dictionary<string, Sprite> CachedSprites = new();

    public static Sprite LoadSprite(string file, float pixelsPerUnit = 1f)
    {
        try
        {
            if (CachedSprites.TryGetValue(file + pixelsPerUnit, out var sprite)) return sprite;
            Texture2D texture = LoadTextureFromResources(file);
            sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return CachedSprites[file + pixelsPerUnit] = sprite;
        }
        catch
        {
            Logger.Error($"读入Texture失败：{file}", "LoadImage");
        }
        return null;
    }

    public static Texture2D LoadTextureFromResources(string file)
    {
        var path = @"FinalSuspect_Data/Resources/Images/" + file;

        try
        {
            if (!File.Exists(path))
            {
                Logger.Warn($"文件不存在：{path}", "LoadTexture");
                goto InDLL;
            }

            byte[] fileData = File.ReadAllBytes(path);
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            if (ImageConversion.LoadImage(texture, fileData))
            {
                return texture;
            }
            else
            {
                Logger.Warn($"无法读取图片：{path}", "LoadTexture");
            }
        }
        catch (Exception ex)
        {
            Logger.Warn($"读入Texture失败：{path} - {ex.Message}", "LoadTexture");
        }
        InDLL:
        path = "FinalSuspect_Xtreme.Resources.Images." + file;

        try
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            using MemoryStream ms = new();
            stream.CopyTo(ms);
            ImageConversion.LoadImage(texture, ms.ToArray(), false);
            return texture;
        }
        catch
        {
            Logger.Error($"读入Texture失败：{path}", "LoadImage");
        }
        return null;
    }

    public static string ColorString(Color32 color, string str) => $"<color=#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}>{str}</color>";
    /// <summary>
    /// Darkness:１の比率で黒色と元の色を混ぜる。マイナスだと白色と混ぜる。
    /// </summary>
    public static Color ShadeColor(this Color color, float Darkness = 0)
    {
        bool IsDarker = Darkness >= 0; //黒と混ぜる
        if (!IsDarker) Darkness = -Darkness;
        float Weight = IsDarker ? 0 : Darkness; //黒/白の比率
        float R = (color.r + Weight) / (Darkness + 1);
        float G = (color.g + Weight) / (Darkness + 1);
        float B = (color.b + Weight) / (Darkness + 1);
        return new Color(R, G, B, color.a);
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
        or "canneddrum#2370" //喜
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
        if (player == null) player = XtremeGameData.XtremePlayerData.GetPlayerById(playerId);
        cachedPlayers[playerId] = player;
        return player;
    }


    public static string GetProgressText(PlayerControl pc = null)
    {
        pc ??= PlayerControl.LocalPlayer;

        var enable = CanSeeOthersRole(pc, out var bothImp) || bothImp;


        var comms = IsActive(SystemTypes.Comms);
        string text = GetProgressText(pc.PlayerId, comms);
        return enable? text : "";
    }
    private static string GetProgressText(byte playerId, bool comms = false)
    {
        return GetTaskProgressText(playerId, comms);
    }
    public static string GetTaskProgressText(byte playerId, bool comms = false)
    {
        var data = XtremeGameData.XtremePlayerData.GetPlayerDataById(playerId);
        if (!XtremeGameData.GameStates.IsNormalGame)
        {
            if (data.IsImpostor)
            {
                var KillColor = Palette.ImpostorRed;
                return ColorString(KillColor, $"({GetString("KillCount")}: {data.KillCount})");
            }
            return "";
        }

        
        if (data.IsImpostor)
        {
            var KillColor = data.IsDisconnected ? Color.gray : Palette.ImpostorRed;
            return ColorString(KillColor, $"({GetString("KillCount")}: {data.KillCount})");
        }
        var NormalColor = data.TaskCompleted ? Color.green : Color.yellow;
        Color TextColor = comms || data.IsDisconnected ? Color.gray : NormalColor;
        string Completed = comms ? "?" : $"{data.CompleteTaskCount}";
        return ColorString(TextColor, $"({Completed}/{data.TotalTaskCount})");

    }
    public static string GetVitalText(byte playerId,  bool summary = false)
    {
        var data = XtremeGameData.XtremePlayerData.GetPlayerDataById(playerId);
        if (!data.IsDead) return "";
        if (data.IsDead && data.RealDeathReason == DataDeathReason.None)
            data.SetDeathReason(DataDeathReason.Exile);//WarpUp未执行时的处理

        string deathReason = GetString("DeathReason." + data.RealDeathReason);
        Color color = Palette.CrewmateBlue;

        switch (data.RealDeathReason)
        {
            case DataDeathReason.Disconnect:
                color = Palette.DisabledGrey;
                break;
            case DataDeathReason.Kill:
                color = Palette.ImpostorRed;
                var killercolor = Palette.PlayerColors[data.RealKiller.PlayerColor];

                if (summary)
                    deathReason += $"<=<size=80%>{ColorString(killercolor, data.RealKiller.PlayerName)}</size>";
                else
                    deathReason = ColorString(killercolor, deathReason);
                break;
            case DataDeathReason.Exile:
                color = Palette.Purple;
                break;
        }

        if (!summary) deathReason = "(" + deathReason + ")";

        deathReason = ColorString(color, deathReason);


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
                    else
                    {
                        var ReactorSystemType = ShipStatus.Instance.Systems[type].Cast<ReactorSystemType>();
                        return ReactorSystemType != null && ReactorSystemType.IsActive;
                    }
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
                    if (mapId is 1 or 3 or 5)
                    {
                        var HqHudSystemType = ShipStatus.Instance.Systems[type].Cast<HqHudSystemType>();
                        return HqHudSystemType != null && HqHudSystemType.IsActive;
                    }
                    else
                    {
                        var HudOverrideSystemType = ShipStatus.Instance.Systems[type].Cast<HudOverrideSystemType>();
                        return HudOverrideSystemType != null && HudOverrideSystemType.IsActive;
                    }
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
    public static bool CanSeeOthersRole(PlayerControl target, out bool bothImp)
    {
        var LocalDead = !PlayerControl.LocalPlayer.IsAlive();
        var IsAngel = PlayerControl.LocalPlayer.GetRoleType() is RoleTypes.GuardianAngel;
        var BothDeathCanSee = LocalDead && ((!target.IsAlive() && IsAngel) || !IsAngel);
        bothImp = PlayerControl.LocalPlayer.IsImpostor() && target.IsImpostor();


        if (target.IsLocalPlayer() ||
            BothDeathCanSee ||
            bothImp && LocalDead)
            return true;
        return false;
    }

}