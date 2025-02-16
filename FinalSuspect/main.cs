using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using FinalSuspect;
using FinalSuspect.Attributes;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Modules.Random;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;


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
    public const string PluginVersion = "1.1.0";
    public const string PluginGuid = "cn.finalsuspect.xtremewave";
    public const int PluginCreation = 0;

    // == 认证设定 / Authentication Config ==
    public static HashAuth DebugKeyAuth { get; private set; }
    public const string DebugKeyHash = "c0fd562955ba56af3ae20d7ec9e64c664f0facecef4b3e366e109306adeae29d";
    public const string DebugKeySalt = "59687b";
    public static ConfigEntry<string> DebugKeyInput { get; private set; }
    // == 版本相关设定 / Version Config ==
    public const string LowestSupportedVersion = "2024.10.29";


    public const string DisplayedVersion_Head = "1.1";
    private static string DisplayedVersion_Date

    {
        get
        {
#if DEBUG
            var currentDate = DateTime.Now;
            var year = currentDate.Year.ToString();
            var month = currentDate.Month.ToString("D2");  
            var day = currentDate.Day.ToString("D2");    
            return $"{year}{month}{day}";
#else
            return "20240216";
#endif
        }
    }
    


        

    /// <summary>
    /// 测试信息；
    /// 支持的内容：Alpha, Beta, Canary, Dev, RC, Preview, Scrapter
    /// Alpha: 早期内测版
    /// Beta: 内测版
    /// Canary: 测试版(不稳定)
    /// Dev: 开发版
    /// RC: 发行候选版Release Candidate
    /// Preview: 预览/预发行版
    /// Scrapter: 废弃版
    /// </summary>
    private const VersionTypes DisplayedVersion_TestText = VersionTypes.Release;

    private const int DisplayedVersion_TestCreation = 0;
    
    public static readonly string DisplayedVersion = 
        $"{DisplayedVersion_Head}_{DisplayedVersion_Date}" +
        $"{(DisplayedVersion_TestText != VersionTypes.Release ? 
            $"_{DisplayedVersion_TestText}_{DisplayedVersion_TestCreation}" : "")}";


    // == 链接相关设定 / Link Config ==
    //public static readonly string WebsiteUrl = IsChineseLanguageUser ? "https://www.xtreme.net.cn/project/FS/" : "https://www.xtreme.net.cn/en/project/FS/";
    public static readonly string QQInviteUrl = "https://qm.qq.com/q/GNbm9UjfCa";
    public static readonly string DiscordInviteUrl = "https://discord.gg/kz787Zg7h8/";
    public static readonly string GithubRepoUrl = "https://github.com/XtremeWave/FinalSuspect/";

    // ==========
    public Harmony Harmony { get; } = new (PluginGuid);
    public static Version version = Version.Parse(PluginVersion);
    public static ManualLogSource Logger;
    public static bool hasArgumentException;
    public static string ExceptionMessage;
    public static bool ExceptionMessageIsShown;
    public static string CredentialsText;
    public static NormalGameOptionsV08 NormalOptions => GameOptionsManager.Instance.currentNormalGameOptions;
    public static HideNSeekGameOptionsV08 HideNSeekOptions => GameOptionsManager.Instance.currentHideNSeekGameOptions;

    //Client Options
    public static ConfigEntry<bool> KickPlayerWhoFriendCodeNotExist { get; private set; }
    public static ConfigEntry<bool> KickPlayerWithDenyName { get; private set; }
    public static ConfigEntry<bool> KickPlayerInBanList { get; private set; }
    public static ConfigEntry<bool> SpamDenyWord { get; private set; }
    public static ConfigEntry<bool> UnlockFPS { get; private set; }
    public static ConfigEntry<string> ChangeOutfit { get; private set; }
    public static ConfigEntry<bool> AutoStartGame { get; private set; }
    public static ConfigEntry<bool> AutoEndGame { get; private set; }
    public static ConfigEntry<bool> DisableVanillaSound { get; private set; }
    public static ConfigEntry<bool> DisableFAC { get; private set; }
    public static ConfigEntry<bool> PrunkMode { get; private set; }
    public static ConfigEntry<bool> ShowPlayerInfo { get; private set; }
    public static ConfigEntry<bool> UseModCursor { get; private set; }
    public static ConfigEntry<bool> FastBoot { get; private set; }
    public static ConfigEntry<bool> VersionCheat { get; private set; }
    public static ConfigEntry<bool> GodMode { get; private set; }
    public static ConfigEntry<bool> NoGameEnd { get; private set; }
    

    public static readonly string[] OutfitType =
    [
        "BeanMode", "HorseMode", "LongMode"
    ];
    //Other Configs
    public static ConfigEntry<string> HideName { get; private set; }
    public static ConfigEntry<string> HideColor { get; private set; }
    public static ConfigEntry<bool> ShowResults { get; private set; }
    public static ConfigEntry<string> WebhookURL { get; private set; }
    public static ConfigEntry<bool> EnableFinalSuspect { get; private set; }
    public static ConfigEntry<string> LastStartVersion { get; private set; }


    public static Dictionary<RoleTypes, string> roleColors;
    public static List<int> clientIdList = [];

    public static string HostNickName = "";
    public static bool IsInitialRelease = DateTime.Now.Month == 8 && DateTime.Now.Day is 14;
    public static bool IsAprilFools = DateTime.Now.Month == 4 && DateTime.Now.Day is 1;
    public const float RoleTextSize = 2f;

    public static IEnumerable<PlayerControl> AllPlayerControls => 
        PlayerControl.AllPlayerControls.ToArray().Where(p => p != null);
    public static IEnumerable<PlayerControl> AllAlivePlayerControls => 
        PlayerControl.AllPlayerControls.ToArray().Where(p => p != null && p.IsAlive() && !p.Data.Disconnected);

    public static Main Instance;

    public static bool NewLobby = false;

    public static List<string> TName_Snacks_CN =
    [
        "冰激凌", "奶茶", "巧克力", "蛋糕", "甜甜圈", "可乐", "柠檬水", "冰糖葫芦", "果冻", "糖果", "牛奶",
        "抹茶", "烧仙草", "菠萝包", "布丁", "椰子冻", "曲奇", "红豆土司", "三彩团子", "艾草团子", "泡芙", "可丽饼",
        "桃酥", "麻薯", "鸡蛋仔", "马卡龙", "雪梅娘", "炒酸奶", "蛋挞", "松饼", "西米露", "奶冻", "奶酥", "可颂", "奶糖"
    ];
    public static List<string> TName_Snacks_EN =
    [
        "Ice cream", "Milk tea", "Chocolate", "Cake", "Donut", "Coke", "Lemonade", "Candied haws", "Jelly", "Candy",
        "Milk",
        "Matcha", "Burning Grass Jelly", "Pineapple Bun", "Pudding", "Coconut Jelly", "Cookies", "Red Bean Toast",
        "Three Color Dumplings", "Wormwood Dumplings", "Puffs", "Can be Crepe", "Peach Crisp", "Mochi", "Egg Waffle",
        "Macaron",
        "Snow Plum Niang", "Fried Yogurt", "Egg Tart", "Muffin", "Sago Dew", "panna cotta", "soufflé", "croissant",
        "toffee"
    ];
    public static string Get_TName_Snacks => TranslationController.Instance.currentLanguage.languageID is SupportedLangs.SChinese or SupportedLangs.TChinese ?
        TName_Snacks_CN[IRandom.Instance.Next(0, TName_Snacks_CN.Count)] :
        TName_Snacks_EN[IRandom.Instance.Next(0, TName_Snacks_EN.Count)];

    public override void Load()
    {
        Instance = this;

        //Client Options
        HideName = Config.Bind("Xtreme System", "Hide Game Code Name", "Final Suspect");
        HideColor = Config.Bind("Xtreme System", "Hide Game Code Color", $"{ColorHelper.ModColor}");
        EnableFinalSuspect = Config.Bind("Xtreme System", "Enable Final Suspect", true);
        ShowResults = Config.Bind("Xtreme System", "Show Results", true);
        LastStartVersion = Config.Bind("Xtreme System", "Last Start Version", "0.0.0");
        
        DebugKeyInput = Config.Bind("Authentication", "Debug Key", "");

        UnlockFPS = Config.Bind("Client Options", "Unlock FPS", false);
        ChangeOutfit = Config.Bind("Client Options", "Change Outfit", OutfitType[0]);
        KickPlayerWhoFriendCodeNotExist = Config.Bind("Client Options", "Kick Player FriendCode Not Exist", true);
        KickPlayerInBanList = Config.Bind("Client Options", "Kick Player In BanList", true);
        KickPlayerWithDenyName = Config.Bind("Client Options", "Kick Player With Deny Name", true);
        SpamDenyWord = Config.Bind("Client Options", "Spam Deny Word", true);
        AutoStartGame = Config.Bind("Client Options", "Auto Start Game", false);
        AutoEndGame = Config.Bind("Client Options", "Auto End Game", false);
        DisableVanillaSound = Config.Bind("Client Options", "Disable Vanilla Sound", false);
        DisableFAC = Config.Bind("Client Options", "Disable FAC", false);
        PrunkMode = Config.Bind("Client Options", "Prunk Mode", false);
        ShowPlayerInfo = Config.Bind("Client Options", "Show Player Info", true);
        UseModCursor = Config.Bind("Client Options", "Use Mod Cursor", true);
        FastBoot = Config.Bind("Client Options", "Fast Boot", false);
        VersionCheat = Config.Bind("Client Options", "Version Cheat", false);
        GodMode = Config.Bind("Client Options", "God Mode", false);
        NoGameEnd = Config.Bind("Client Options", "No Game End", false);

        Logger = BepInEx.Logging.Logger.CreateLogSource("FinalSuspect");
        XtremeLogger.Enable();
        XtremeLogger.Disable("SwitchSystem");
        XtremeLogger.Disable("ModNews");
        XtremeLogger.Disable("CancelPet");
        if (!DebugModeManager.AmDebugger)
        {
            XtremeLogger.Disable("Download Resources");
        }
        XtremeLogger.isDetail = true;

        // 認証関連-初期化
        DebugKeyAuth = new HashAuth(DebugKeyHash, DebugKeySalt);

        // 認証関連-認証
        DebugModeManager.Auth(DebugKeyAuth, DebugKeyInput.Value);

        WebhookURL = Config.Bind("hook", "WebhookURL", "none");

        hasArgumentException = false;
        ExceptionMessage = "";
        try
        {
            roleColors = new Dictionary<RoleTypes, string>
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
                { RoleTypes.Phantom, "#CA8AFF" },
            };
        }
        catch (ArgumentException ex)
        {
            XtremeLogger.Error("错误：字典出现重复项", "LoadDictionary");
            XtremeLogger.Exception(ex, "LoadDictionary");
            hasArgumentException = true;
            ExceptionMessage = ex.Message;
            ExceptionMessageIsShown = false;
        }

        RegistryManager.Init(); // 这是优先级最高的模块初始化方法，不能使用模块初始化属性

        PluginModuleInitializerAttribute.InitializeAll();

        IRandom.SetInstance(new NetRandomWrapper());

        XtremeLogger.Info($"{Application.version}", "AmongUs Version");

        var handler = XtremeLogger.Handler("GitVersion");
        handler.Info($"{nameof(ThisAssembly.Git.BaseTag)}: {ThisAssembly.Git.BaseTag}");
        handler.Info($"{nameof(ThisAssembly.Git.Commit)}: {ThisAssembly.Git.Commit}");
        handler.Info($"{nameof(ThisAssembly.Git.Commits)}: {ThisAssembly.Git.Commits}");
        handler.Info($"{nameof(ThisAssembly.Git.IsDirty)}: {ThisAssembly.Git.IsDirty}");
        handler.Info($"{nameof(ThisAssembly.Git.Sha)}: {ThisAssembly.Git.Sha}");
        handler.Info($"{nameof(ThisAssembly.Git.Tag)}: {ThisAssembly.Git.Tag}");

        ClassInjector.RegisterTypeInIl2Cpp<ErrorText>();

        Task.Run(SystemEnvironment.SetEnvironmentVariablesAsync);

        Harmony.PatchAll();
        
        if (DebugModeManager.AmDebugger) ConsoleManager.CreateConsole();
        else ConsoleManager.DetachConsole();

        XtremeLogger.Msg("========= FinalSuspect loaded! =========", "Plugin Load");
        Application.quitting += new Action(Utils.SaveNowLog);
    }
}

public enum VersionTypes
{
    Alpha,// 早期内测版
    Beta,// 内测版
    Canary,// 测试版(不稳定)
    Dev,// 开发版
    RC,// 发行候选版Release Candidate
    Preview,// 预览/预发行版
    Scrapter,// 废弃版
    Release,// 发行版
}