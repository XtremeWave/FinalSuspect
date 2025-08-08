using System;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using FinalSuspect;
using FinalSuspect.Attributes;
using FinalSuspect.DataHandling.FinalAntiCheat.Core;
using FinalSuspect.Helpers;
using FinalSuspect.Internal;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using FinalSuspect.Modules.Random;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global

[assembly: AssemblyFileVersion(Main.PluginVersion)]
[assembly: AssemblyInformationalVersion(Main.PluginVersion)]
[assembly: AssemblyVersion(Main.PluginVersion)]

namespace FinalSuspect;

[BepInPlugin(PluginGuid, "FinalSuspect", PluginVersion)]
[BepInProcess("Among Us.exe")]
public class Main : BasePlugin
{
    // == 程序基本设定 / Program Config ==
    public const string ModName = "Final Suspect";
    public const string ForkId = "Final Suspect";
    public const string PluginVersion = "1.2.0";
    public const string PluginGuid = "cn.slok.finalsuspect";
    public const int PluginCreation = 0;
    public const string DebugKeyHash = "c0fd562955ba56af3ae20d7ec9e64c664f0facecef4b3e366e109306adeae29d";
    public const string DebugKeySalt = "59687b";

    // == 版本相关设定 / Version Config ==
    public const string LowestSupportedVersion = "2025.6.10"; // 16.1.0

    private const string DisplayedVersion_Head = "1.2";

    private const string DisplayedVersion_Date = BuildTime.Date;

    /// <summary>
    ///     表示当前显示的版本类型。
    /// </summary>
    private const VersionTypes DisplayedVersion_Type = VersionTypes.Release;

    private const int DisplayedVersion_TestCreation = 0;


    // == 链接相关设定 / Link Config ==
    //public static readonly string WebsiteUrl = IsChineseLanguageUser ? "https://www.Final.net.cn/project/FS/" : "https://www.Final.net.cn/en/project/FS/";
    public const string QQInviteUrl = "https://qm.qq.com/q/GNbm9UjfCa";
    public const string DiscordInviteUrl = "https://discord.gg/kz787Zg7h8/";
    public const string GithubRepoUrl = "https://github.com/Slok7565/FinalSuspect/";
    public const float RoleTextSize = 2f;

    public static readonly string DisplayedVersion =
#if RELEASE
        $"{DisplayedVersion_Head}_{DisplayedVersion_Date}";
#else
        $"{DisplayedVersion_Head}_{DisplayedVersion_Date}_{DisplayedVersion_Type}_{DisplayedVersion_TestCreation}";
#endif
    public static readonly Version version = Version.Parse(PluginVersion);
    public static ManualLogSource Logger;
    public static bool hasArgumentException;
    public static string ExceptionMessage;
    public static bool ExceptionMessageIsShown;
    public static string CredentialsText;

    public static readonly Dictionary<RoleTypes, string> roleColors = new()
    {
        { RoleTypes.CrewmateGhost, "#8CFFFF" },
        { RoleTypes.GuardianAngel, "#8CFFDB" },
        { RoleTypes.Crewmate, "#8CFFFF" },
        { RoleTypes.Scientist, "#F8FF8C" },
        { RoleTypes.Engineer, "#A5A8FF" },
        { RoleTypes.Noisemaker, "#FFC08C" },
        { RoleTypes.Tracker, "#93FF8C" },
        { RoleTypes.ImpostorGhost, "#FF1919" },
        { RoleTypes.Impostor, "#FF1919" },
        { RoleTypes.Shapeshifter, "#FF819E" },
        { RoleTypes.Phantom, "#CA8AFF" }
    };

    public static string HostNickName = "";
    public static readonly bool IsInitialRelease = DateTime.Now.Month == 8 && DateTime.Now.Day is 15;
    public static readonly bool IsAprilFools = DateTime.Now is { Month: 4, Day: >= 1 and <= 10 };
    public static readonly bool IsValentines = DateTime.Now.Month == 2 && DateTime.Now.Day is 14;

    public static Main Instance;

    // == 认证设定 / Authentication Config ==
    public static HashAuth DebugKeyAuth { get; private set; }
    public static ConfigEntry<string> DebugKeyInput { get; private set; }

    // ==========
    public Harmony Harmony { get; } = new(PluginGuid);
    public static NormalGameOptionsV09 NormalOptions => GameOptionsManager.Instance.currentNormalGameOptions;
    public static HideNSeekGameOptionsV09 HideNSeekOptions => GameOptionsManager.Instance.currentHideNSeekGameOptions;

    //Client Options
    public static ConfigEntry<bool> KickPlayerWithAbnormalFriendCode { get; private set; }
    public static ConfigEntry<bool> KickPlayerWithDenyName { get; private set; }
    public static ConfigEntry<bool> KickPlayerInBanList { get; private set; }
    public static ConfigEntry<bool> SpamDenyWord { get; private set; }
    public static ConfigEntry<bool> UnlockFPS { get; private set; }
    public static ConfigEntry<OutfitType> SwitchOutfitType { get; private set; }
    public static ConfigEntry<bool> AutoStartGame { get; private set; }
    public static ConfigEntry<bool> AutoEndGame { get; private set; }
    public static ConfigEntry<bool> DisableVanillaSound { get; private set; }
    public static ConfigEntry<bool> EnableFAC { get; private set; }
    public static ConfigEntry<bool> EnableGuardian { get; private set; }
    public static ConfigEntry<bool> ShowPlayerInfo { get; private set; }
    public static ConfigEntry<bool> FastLaunchMode { get; private set; }
    public static ConfigEntry<bool> OfflineMode { get; private set; }
    public static ConfigEntry<bool> UseModCursor { get; private set; }
    public static ConfigEntry<bool> VersionCheat { get; private set; }
    public static ConfigEntry<bool> GodMode { get; private set; }
    public static ConfigEntry<bool> NoGameEnd { get; private set; }

    //Other Configs
    public static ConfigEntry<string> HideName { get; private set; }
    public static ConfigEntry<string> HideColor { get; private set; }
    public static ConfigEntry<bool> ShowResults { get; private set; }
    public static ConfigEntry<string> WebhookURL { get; private set; }
    public static ConfigEntry<bool> EnableFinalSuspect { get; private set; }
    public static ConfigEntry<string> LastStartVersion { get; private set; }
    public static ConfigEntry<BypassType> LanguageUpdateBypass { get; private set; }
    public static ConfigEntry<int> CurrentStyleId { get; private set; }

    public static IEnumerable<PlayerControl> AllPlayerControls =>
        PlayerControl.AllPlayerControls.ToArray().Where(p => p);

    public static IEnumerable<PlayerControl> AllAlivePlayerControls =>
        PlayerControl.AllPlayerControls.ToArray().Where(p => p && p.IsAlive() && !p.Data.Disconnected);

    public override void Load()
    {
        Instance = this;

        //Configs
        HideName = Config.Bind("Final System", "Hide Game Code Name", "Final Suspect");
        HideColor = Config.Bind("Final System", "Hide Game Code Color", $"{ColorHelper.FSColorHex}");
        EnableFinalSuspect = Config.Bind("Final System", "Enable Final Suspect", true);
        ShowResults = Config.Bind("Final System", "Show Results", true);
        LastStartVersion = Config.Bind("Final System", "Last Start Version", "0.0.0");
        LanguageUpdateBypass = Config.Bind("Final System", "Language Update Bypass", BypassType.Dont);
        CurrentStyleId = Config.Bind("Final System", "BG Id", 0);

        DebugKeyInput = Config.Bind("Authentication", "Debug Key", "");

        UnlockFPS = Config.Bind("Client Options", "Unlock FPS", false);
        SwitchOutfitType = Config.Bind("Client Options", "Switch Outfit", OutfitType.BeanMode);
        KickPlayerWithAbnormalFriendCode = Config.Bind("Client Options", "Kick Player FriendCode Not Exist", true);
        KickPlayerInBanList = Config.Bind("Client Options", "Kick Player In BanList", true);
        KickPlayerWithDenyName = Config.Bind("Client Options", "Kick Player With Deny Name", true);
        SpamDenyWord = Config.Bind("Client Options", "Spam Deny Word", true);
        AutoStartGame = Config.Bind("Client Options", "Auto Start Game", false);
        AutoEndGame = Config.Bind("Client Options", "Auto End Game", false);
        DisableVanillaSound = Config.Bind("Client Options", "Disable Vanilla Sound", false);
        EnableFAC = Config.Bind("Client Options", "Enable FAC", false);
        EnableGuardian = Config.Bind("Client Options", "Enable Guardian", true);
        //PrunkMode = Config.Bind("Client Options", "Prunk Mode", false);
        ShowPlayerInfo = Config.Bind("Client Options", "Show Player Info", true);
        FastLaunchMode = Config.Bind("Client Options", "Fast Launch Mode", false);
        OfflineMode = Config.Bind("Client Options", "Offline Mode", false);
        UseModCursor = Config.Bind("Client Options", "Use Mod Cursor", true);

        VersionCheat = Config.Bind("Client Options", "Version Cheat", false);
        GodMode = Config.Bind("Client Options", "God Mode", false);
        NoGameEnd = Config.Bind("Client Options", "No Game End", false);

        Logger = BepInEx.Logging.Logger.CreateLogSource("FinalSuspect");
        Enable();
        Disable("SwitchSystem");
        Disable("ModNews");
        Disable("CancelPet");
        if (!DebugModeManager.IsDebugMode)
        {
            Disable("Download Resources");
            Disable("GetAnnouncements");
            Disable("GetConfigs");
        }

        isDetail = true;

        // 認証関連-初期化
        DebugKeyAuth = new HashAuth(DebugKeyHash, DebugKeySalt);

        // 認証関連-認証
        DebugModeManager.Auth(DebugKeyAuth, DebugKeyInput.Value);

        WebhookURL = Config.Bind("hook", "WebhookURL", "none");

        hasArgumentException = false;
        ExceptionMessage = "";

        RegistryManager.Init(); // 这是优先级最高的模块初始化方法，不能使用模块初始化属性
        DllChecker.Init();

        PluginModuleInitializerAttribute.InitializeAll();

        IRandom.SetInstance(new NetRandomWrapper());

        Info($"{Application.version}", "AmongUs Version");

        var handler = Handler("GitVersion");
        handler.Info($"{nameof(GitBaseTag)}: {GitBaseTag}");
        handler.Info($"{nameof(GitCommit)}: {GitCommit}");
        handler.Info($"{nameof(GitCommits)}: {GitCommits}");
        handler.Info($"{nameof(GitIsDirty)}: {GitIsDirty}");
        handler.Info($"{nameof(GitSha)}: {GitSha}");
        handler.Info($"{nameof(GitTag)}: {GitTag}");

        ClassInjector.RegisterTypeInIl2Cpp<ErrorText>();

        Task.Run(SystemEnvironment.SetEnvironmentVariablesAsync);

        Harmony.PatchAll();

        if (DebugModeManager.IsDebugMode) ConsoleManager.CreateConsole();
        else ConsoleManager.DetachConsole();

        Msg("========= FinalSuspect loaded! =========", "Plugin Load");
        Application.quitting += new Action(SaveNowLog);
    }

#pragma warning disable CS0618 // 类型或成员已过时
    public const string GitBaseTag = ThisAssembly.Git.BaseTag;
    public const string GitCommit = ThisAssembly.Git.Commit;
    public const string GitCommits = ThisAssembly.Git.Commits;
    public const string GitBranch = ThisAssembly.Git.Branch;
    public const bool GitIsDirty = ThisAssembly.Git.IsDirty;
    public const string GitSha = ThisAssembly.Git.Sha;
    public const string GitTag = ThisAssembly.Git.Tag;
#pragma warning restore CS0618
}

/// <summary>
///     表示软件版本的不同类型。
/// </summary>
public enum VersionTypes
{
    /// <summary>早期内测版。</summary>
    Alpha,

    /// <summary>内测版。</summary>
    Beta,

    /// <summary>测试版（不稳定）。</summary>
    Canary,

    /// <summary>开发版。</summary>
    Dev,

    /// <summary>发行候选版 (Release Candidate)。</summary>
    RC,

    /// <summary>预览/预发行版。</summary>
    Preview,

    /// <summary>废弃版。</summary>
    Scrapter,

    /// <summary>
    ///     正式发行版。
    ///     除此之外若要发行，全部使用OpenBeta。
    /// </summary>
    Release
}

public enum BypassType
{
    Dont,
    Once,
    LongTerm
}

public enum OutfitType
{
    BeanMode,
    HorseMode,
    LongMode
}