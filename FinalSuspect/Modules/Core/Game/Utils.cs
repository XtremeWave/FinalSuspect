using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using AmongUs.GameOptions;
using FinalSuspect.DataHandling.FinalAntiCheat.Core;
using FinalSuspect.DataHandling.FinalGameData;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using FinalSuspect.Modules.Resources;
using FinalSuspect.Patches.Game_Vanilla;
using FinalSuspect.Patches.System;
using Il2CppInterop.Runtime.InteropTypes;
using InnerNet;
using UnityEngine;

namespace FinalSuspect.Modules.Core.Game;

public static class Utils
{
    private static readonly DateTime timeStampStartTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


    private static readonly Dictionary<byte, PlayerControl> cachedPlayers = new();
    public static long TimeStamp => (long)(DateTime.Now.ToUniversalTime() - timeStampStartTime).TotalSeconds;

    public static long GetTimeStamp(DateTime? dateTime = null)
    {
        return (long)((dateTime ?? DateTime.Now).ToUniversalTime() - timeStampStartTime).TotalSeconds;
    }

    public static float GetResolutionOffset()
    {
        return (float)Screen.width / Screen.height / (16f / 9f);
    }

    private static ClientData GetClientById(int id)
    {
        try
        {
            var client = AmongUsClient.Instance.allClients.ToArray().FirstOrDefault(cd => cd.Id == id);
            return client;
        }
        catch
        {
            return null;
        }
    }

    // ReSharper disable once RedundantAssignment
    public static void KickPlayer(int clientId, bool ban, string reason = "", KickLevel level = KickLevel.Notification)
    {
        if (OnPlayerLeftPatch.ClientsProcessed.Contains(clientId)) return;
        var client = GetClientById(clientId);
        Info($"try to kick {client?.Character?.GetRealName()} Due to {reason}", "Kick Player");
        var _player = FinalPlayerData.AllPlayerData.FirstOrDefault(p => p.CheatData?.ClientData?.Id == clientId)
            ?.Player;
        try
        {
#if DEBUG
            ban = false;
#endif
            OnPlayerLeftPatch.Add(clientId);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            AmongUsClient.Instance.KickPlayer(clientId, ban);
            if (level != KickLevel.None)
                NotificationPopperPatch.NotificationPop(string.Format(GetString($"{level}.{reason}"),
                    _player ? _player.GetColoredName() : client?.PlayerName));
        }
        catch
        {
            /* ignored */
        }
    }

    public static void KickPlayer(byte playerId, bool ban, string reason = "",
        KickLevel level = KickLevel.CheatDetected)
    {
        try
        {
            KickPlayer(GetPlayerById(playerId).GetClient().Id, ban, reason, level);
        }
        catch
        {
            /* ignored */
        }
    }

    public static string PadRightV2(this object text, int num)
    {
        var t = text.ToString();
        var bc = t!.Sum(c => Encoding.GetEncoding("UTF-8").GetByteCount(c.ToString()) == 1 ? 1 : 2);
        return t.PadRight(Mathf.Max(num - (bc - t.Length), 0));
    }

    /// <summary>
    ///     乱数の簡易的なヒストグラムを取得する関数
    ///     <params name="nums">生成した乱数を格納したint配列</params>
    ///     <params name="scale">ヒストグラムの倍率 大量の乱数を扱う場合、この値を下げることをお勧めします。</params>
    /// </summary>
    public static bool AmDev()
    {
        return IsDev(EOSManager.Instance.FriendCode);
    }

    public static bool IsDev(string friendCode)
    {
        return friendCode
            is "teamelder#5856" //Slok
            or "cloakhazy#9133"; //LezaiYa
    }

    public static PlayerControl GetPlayerById(int playerId)
    {
        return GetPlayerById((byte)playerId);
    }

    public static PlayerControl GetPlayerById(byte playerId)
    {
        if (cachedPlayers.TryGetValue(playerId, out var cachedPlayer) && cachedPlayer) return cachedPlayer;

        var player = Main.AllPlayerControls.FirstOrDefault(pc => pc.PlayerId == playerId);
        cachedPlayers[playerId] = player;
        return player;
    }

    public static void ExecuteWithTryCatch(this Action action, bool Log = false)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            if (Log) Error(ex.ToString(), "Execute With Try Catch");
        }
    }

    public static void FormatButtonColor(MainMenuManager __instance, PassiveButton button, Color inActiveColor,
        Color activeColor, Color inActiveTextColor, Color activeTextColor)
    {
        button.activeSprites.transform.FindChild("Shine")?.gameObject.SetActive(false);
        button.inactiveSprites.transform.FindChild("Shine")?.gameObject.SetActive(false);
        var activeRenderer = button.activeSprites.GetComponent<SpriteRenderer>();
        var inActiveRenderer = button.inactiveSprites.GetComponent<SpriteRenderer>();
        activeRenderer.sprite = __instance.quitButton.activeSprites.GetComponent<SpriteRenderer>().sprite;
        inActiveRenderer.sprite = __instance.quitButton.activeSprites.GetComponent<SpriteRenderer>().sprite;
        activeRenderer.color = activeColor.a == 0f
            ? new Color(inActiveColor.r, inActiveColor.g, inActiveColor.b, 1f)
            : activeColor;
        inActiveRenderer.color = inActiveColor;
        button.activeTextColor = activeTextColor;
        button.inactiveTextColor = inActiveTextColor;
    }

    public static long GetCurrentTimestamp()
    {
        return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }

    #region Log Out Put

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
        if (!PlayerControl.LocalPlayer) return;
        var t = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
        if (popup)
            HudManager.Instance.ShowPopUp(string.Format(GetString("Message.DumpfileSaved"),
                $"FinalSuspect - v{Main.DisplayedVersion}-{t}.log"));
        else
            AddChatMessage(string.Format(GetString("Message.DumpfileSaved"),
                $"FinalSuspect - v{Main.DisplayedVersion}-{t}.log"));
    }

    public static void ClearAutoLogs()
    {
        foreach (var f in Directory.GetFiles(GetLogFolder(true).FullName)) File.Delete(f);
    }

    public static void SaveNowLog()
    {
        var logs = GetLogFolder(true);
        logs.EnumerateFiles().Where(f => f.CreationTime < DateTime.Now.AddDays(-7)).ToList().ForEach(f => f.Delete());
        CopyLog(logs.FullName);
    }

    public static string CopyLog(string path)
    {
        var f = $"{path}";
        var t = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
        var fileName = $"{f}FinalSuspect-v{Main.DisplayedVersion}-{t}.log";
        if (!Directory.Exists(f)) Directory.CreateDirectory(f);
        FileInfo file = new($"{Environment.CurrentDirectory}/BepInEx/LogOutput.log");
        var logFile = file.CopyTo(fileName);
        return logFile.FullName;
    }

    public static void OpenDirectory(string path)
    {
        Process.Start("Explorer.exe", $"/select,{path}");
    }

    private static void AddChatMessage(string text, string title = "")
    {
        if (!AmongUsClient.Instance.AmHost) return;
        var player = PlayerControl.LocalPlayer;
        var name = player.Data.PlayerName;
        player.SetName(title + '\0');
        DestroyableSingleton<HudManager>.Instance?.Chat?.AddChat(player, text);
        player.SetName(name);
    }

    #endregion

    #region Sprite

    private static readonly Dictionary<string, Sprite> CachedSprites = new();

    public static Sprite LoadSprite(string file, float pixelsPerUnit = 1f)
    {
        try
        {
            if (CachedSprites.TryGetValue(file + pixelsPerUnit, out var sprite)) return sprite;
            var texture = LoadTextureFromResources(file);
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f),
                pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return CachedSprites[file + pixelsPerUnit] = sprite;
        }
        catch
        {
            Error($"读入Texture失败：{file}", "LoadImage");
        }

        return null;
    }

    public static Texture2D LoadTextureFromResources(string file)
    {
        var path = GetResourceFilesPath(FileType.Images, file);

        try
        {
            if (!File.Exists(path))
                goto InDLL;

            var fileData = File.ReadAllBytes(path);
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            if (texture.LoadImage(fileData)) return texture;

            Warn($"无法读取图片：{path}", "LoadTexture");
        }
        catch (Exception ex)
        {
            Warn($"读入Texture失败：{path} - {ex.Message}", "LoadTexture");
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
            Error($"读入Texture失败：{path}", "LoadImage");
        }*/
        return null;
    }

    #endregion

    #region Game Play

    public static string GetProgressText(PlayerControl pc = null)
    {
        pc ??= PlayerControl.LocalPlayer;

        var enable = CanSeeTargetRole(pc, out var bothImp) || bothImp;

        var comms = IsActive(SystemTypes.Comms);
        var text = GetProgressText(pc.PlayerId, comms);
        return enable ? text : "";
    }

    private static string GetProgressText(byte playerId, bool comms = false)
    {
        var data = GetFinalDataById(playerId);
        if (!IsNormalGame)
        {
            if (!data.IsImpostor) return "";
            var KillColor = Palette.ImpostorRed;
            return StringHelper.ColorString(KillColor, $"({GetString("KillCount")}: {data.ProcessInt})");
        }

        if (data.IsImpostor)
        {
            var KillColor = data.IsDisconnected ? Color.gray : Palette.ImpostorRed;
            return StringHelper.ColorString(KillColor, $"({GetString("KillCount")}: {data.ProcessInt})");
        }

        var NormalColor = data.TaskCompleted ? Color.green : Color.yellow;
        var TextColor = comms || data.IsDisconnected ? Color.gray : NormalColor;
        var Completed = comms ? "?" : $"{data.ProcessInt}";
        return StringHelper.ColorString(TextColor, $"({Completed}/{data.TotalTaskCount})");
    }

    public static string GetVitalText(byte playerId, bool summary = false, bool doColor = true)
    {
        var data = GetFinalDataById(playerId);
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
                var killerColor = Palette.PlayerColors[data.RealKiller.ColorId];

                if (summary)
                    deathReason += $"<=<size=80%>{StringHelper.ColorString(killerColor, data.RealKiller.Name)}</size>";
                else if (doColor)
                    deathReason = StringHelper.ColorString(killerColor, deathReason);
                break;
            case VanillaDeathReason.Exile:
                color = Palette.Purple;
                break;
        }

        if (!summary) deathReason = "(" + deathReason + ")";

        deathReason = StringHelper.ColorString(color, deathReason);

        return deathReason;
    }

    public static bool IsActive(SystemTypes type)
    {
        if (!IsNormalGame) return false;
        if (!ShipStatus.Instance.Systems.ContainsKey(type)) return false;

        int mapId = Main.NormalOptions.MapId;
        switch (type)
        {
            case SystemTypes.Electrical:
            {
                var SwitchSystem = ShipStatus.Instance.Systems[type].Cast<SwitchSystem>();
                return SwitchSystem is { IsActive: true };
            }
            case SystemTypes.Reactor:
            {
                if (mapId == 2) return false;
                var ReactorSystemType = ShipStatus.Instance.Systems[type].Cast<ReactorSystemType>();
                return ReactorSystemType is { IsActive: true };
            }
            case SystemTypes.Laboratory:
            {
                if (mapId != 2) return false;
                var ReactorSystemType = ShipStatus.Instance.Systems[type].Cast<ReactorSystemType>();
                return ReactorSystemType is { IsActive: true };
            }
            case SystemTypes.LifeSupp:
            {
                if (mapId is 2 or 4) return false;
                var LifeSuppSystemType = ShipStatus.Instance.Systems[type].Cast<LifeSuppSystemType>();
                return LifeSuppSystemType is { IsActive: true };
            }
            case SystemTypes.Comms:
            {
                if (mapId is 1 or 5)
                {
                    var HqHudSystemType = ShipStatus.Instance.Systems[type].Cast<HqHudSystemType>();
                    return HqHudSystemType is { IsActive: true };
                }

                var HudOverrideSystemType = ShipStatus.Instance.Systems[type].Cast<HudOverrideSystemType>();
                return HudOverrideSystemType is { IsActive: true };
            }
            case SystemTypes.HeliSabotage:
            {
                var HeliSabotageSystem = ShipStatus.Instance.Systems[type].Cast<HeliSabotageSystem>();
                return HeliSabotageSystem && HeliSabotageSystem.IsActive;
            }
            case SystemTypes.MushroomMixupSabotage:
            {
                var mushroomMixupSabotageSystem =
                    ShipStatus.Instance.Systems[type].TryCast<MushroomMixupSabotageSystem>();
                return mushroomMixupSabotageSystem && mushroomMixupSabotageSystem.IsActive;
            }
            default:
                return false;
        }
    }

    public static RoleTypes GetRoleType(byte id)
    {
        return GetRoleById(id);
    }

    public static bool IsImpostor(RoleTypes role)
    {
        return role switch
        {
            RoleTypes.Impostor or RoleTypes.Shapeshifter or RoleTypes.Phantom or RoleTypes.ImpostorGhost => true,
            _ => false
        };
    }

    public static bool IsGhost(RoleTypes role)
    {
        return role switch
        {
            RoleTypes.ImpostorGhost or RoleTypes.CrewmateGhost or RoleTypes.GuardianAngel => true,
            _ => false
        };
    }

    public static bool CanSeeTargetRole(PlayerControl target, out bool bothImp)
    {
        var LocalDead = !PlayerControl.LocalPlayer.IsAlive();
        var IsAngel = PlayerControl.LocalPlayer.GetRoleType() is RoleTypes.GuardianAngel;
        var BothDeathCanSee = LocalDead && ((!target.IsAlive() && IsAngel) || !IsAngel);
        bothImp = PlayerControl.LocalPlayer.IsImpostor() && target.IsImpostor();

        return target.IsLocalPlayer() ||
               BothDeathCanSee ||
               (bothImp && LocalDead) ||
               Main.GodMode.Value ||
               IsFreePlay;
    }

    public static bool CanSeeOthersRole()
    {
        if (!IsInGame) return true;
        if (IsFreePlay) return true;
        var LocalDead = !PlayerControl.LocalPlayer.IsAlive();
        var IsAngel = PlayerControl.LocalPlayer.GetRoleType() is RoleTypes.GuardianAngel;

        return (!IsAngel && LocalDead) ||
               Main.GodMode.Value ||
               IsFreePlay;
    }

    public static string GetRoleName(RoleTypes role)
    {
        return GetRoleString(Enum.GetName(typeof(RoleTypes), role));
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
        if (IsNormalGame) return GetString($"{text}{Info}");

        if (InfoLong)
            switch (role)
            {
                case RoleTypes.Engineer:
                    return $"{GetString(StringNames.RuleOneCrewmates)}" +
                           $"\n{GetString(StringNames.RuleTwoCrewmates)}" +
                           $"\n{GetString(StringNames.RuleThreeCrewmates)}";
                case RoleTypes.Impostor:
                    return $"{GetString(StringNames.RuleOneImpostor)}" +
                           $"\n{GetString(StringNames.RuleTwoImpostor)}" +
                           $"\n{GetString(StringNames.RuleThreeImpostor)}";
            }

        text = "HnS" + text;
        return GetString($"{text}{Info}");
    }

    public static string SummaryTexts(byte id)
    {
        var thisData = GetFinalDataById(id);

        var builder = new StringBuilder();
        var longestNameByteCount = GetLongestNameByteCount();

        var pos = Math.Min((float)longestNameByteCount / 2 + 1.5f, 11.5f);

        var colorId = thisData.ColorId;
        builder.Append(StringHelper.ColorString(Palette.PlayerColors[colorId], thisData.Name));
        pos += 1.5f;
        builder.Append($"<pos={pos}em>").Append(GetProgressText(id)).Append("</pos>");
        pos += 4.5f;

        builder.Append($"<pos={pos}em>").Append(GetVitalText(id, true)).Append("</pos>");
        pos += DestroyableSingleton<TranslationController>.Instance.currentLanguage.languageID == SupportedLangs.English
            ? 14f
            : 10.5f;

        builder.Append($"<pos={pos}em>");

        var oldRole = thisData.RoleWhenAlive ?? RoleTypes.Crewmate;
        var newRole = thisData.RoleAfterDeath ??
                      (thisData.IsImpostor ? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost);
        builder.Append(StringHelper.ColorString(GetRoleColor(oldRole), GetRoleString($"{oldRole}")));

        if (thisData.IsDead && newRole != oldRole)
            builder.Append($"=> {StringHelper.ColorString(GetRoleColor(newRole), GetRoleString($"{newRole}"))}");

        builder.Append("</pos>");

        return builder.ToString();
    }

    private static int GetLongestNameByteCount()
    {
        return FinalPlayerData.AllPlayerData.Select(data => data.Name.GetByteCount())
            .OrderByDescending(byteCount => byteCount).FirstOrDefault();
    }

    #endregion

    #region FinalGameData

    public static bool ModClient(int id)
    {
        return GetPlayerVersion(id, out _);
    }

    public static bool OtherModClient(int id)
    {
        return GetPlayerVersion(id, out var ver) && Main.ForkId != ver.forkId;
    }

    public static bool IsFinalSuspect(int id)
    {
        return FinalGameData.PlayerVersion.playerVersion.TryGetValue(id, out var ver) && Main.ForkId == ver.forkId;
    }

    public static bool GetPlayerVersion(int id, out FinalGameData.PlayerVersion ver)
    {
        return FinalGameData.PlayerVersion.playerVersion.TryGetValue(id, out ver) && ver != null;
    }

    #endregion

    #region Final Player Data

    public static FinalPlayerData GetFinalDataById(byte id)
    {
        try
        {
            return FinalPlayerData.AllPlayerData.FirstOrDefault(data => data.PlayerId == id);
        }
        catch
        {
            return null;
        }
    }

    public static string GetPlayerNameById(byte id)
    {
        return GetFinalDataById(id).Name;
    }

    public static RoleTypes GetRoleById(byte id)
    {
        var data = GetFinalDataById(id);
        var dead = data?.IsDead ?? false;
        RoleTypes nullRole;
        if (dead && !IsFreePlay)
            nullRole = data.IsImpostor ? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost;
        else
            nullRole = GetFinalDataById(id).Player.Data.Role.Role;
        var role = (dead ? data.RoleAfterDeath : data?.RoleWhenAlive) ?? nullRole;
        return role;
    }

    public static PlayerCheatData GetCheatDataById(byte id)
    {
        try
        {
            return GetFinalDataById(id)?.CheatData;
        }
        catch
        {
            return null;
        }
    }

    #endregion
}

public enum KickLevel
{
    None,
    Notification,
    CheatDetected
}