using AmongUs.GameOptions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FinalSuspect.Attributes;
using FinalSuspect.Modules;
using UnityEngine;
using FinalSuspect.Modules.Managers;

[assembly: AssemblyFileVersion(FinalSuspect.Main.PluginVersion)]
[assembly: AssemblyInformationalVersion(FinalSuspect.Main.PluginVersion)]
[assembly: AssemblyVersion(FinalSuspect.Main.PluginVersion)]
namespace FinalSuspect;

[BepInPlugin(PluginGuid, "FinalSuspect", PluginVersion)]
[BepInProcess("Among Us.exe")]
public class Main : BasePlugin
{
    // == 程序基本设定 / Program Config ==
    public static readonly string ModName = "Final Suspect";
    public const string ForkId = "Final Suspect";
    public const string PluginVersion = "1.0.0";
    public const string PluginGuid = "cn.finalsuspect.xtremewave";
    public const int PluginCreation = 2;

    // == 认证设定 / Authentication Config ==
    public static HashAuth DebugKeyAuth { get; private set; }
    public const string DebugKeyHash = "c0fd562955ba56af3ae20d7ec9e64c664f0facecef4b3e366e109306adeae29d";
    public const string DebugKeySalt = "59687b";
    public static ConfigEntry<string> DebugKeyInput { get; private set; }
    // == 版本相关设定 / Version Config ==
    public const string LowestSupportedVersion = "2024.10.29";

    public const string DisplayedVersion_Head = "1.0";
    public const string DisplayedVersion_Date = "20241201";
    /// <summary>
    /// 测试信息；
    /// 支持的内容：Alpha, Beta, Canary, Dev, Pre-release
    /// </summary>
    public const string DisplayedVersion_TestText = "Dev";
    public const int DisplayedVersion_TestCreation = 55;
    public static readonly string DisplayedVersion = 
        $"{DisplayedVersion_Head}_{DisplayedVersion_Date}" +
        $"{(DisplayedVersion_TestText != "" ? $"_{DisplayedVersion_TestText}_{DisplayedVersion_TestCreation}" : "")}";


    // == 链接相关设定 / Link Config ==
    public static readonly string WebsiteUrl = Translator.IsChineseLanguageUser ? "https://www.xtreme.net.cn/project/FS/" : "https://www.xtreme.net.cn/en/project/FS/";
    public static readonly string QQInviteUrl = "https://qm.qq.com/q/GNbm9UjfCa";
    public static readonly string DiscordInviteUrl = "https://discord.gg/kz787Zg7h8";
    public static readonly string GithubRepoUrl = "https://github.com/XtremeWave/FinalSuspect";

    // ==========
    public Harmony Harmony { get; } = new Harmony(PluginGuid);
    public static Version version = Version.Parse(PluginVersion);
    public static BepInEx.Logging.ManualLogSource Logger;
    public static bool hasArgumentException = false;
    public static string ExceptionMessage;
    public static bool ExceptionMessageIsShown = false;
    public static string CredentialsText;
    public static NormalGameOptionsV08 NormalOptions => GameOptionsManager.Instance.currentNormalGameOptions;
    public static HideNSeekGameOptionsV08 HideNSeekOptions => GameOptionsManager.Instance.currentHideNSeekGameOptions;

    //Client Options
    public static ConfigEntry<bool> KickPlayerFriendCodeNotExist { get; private set; }
    public static ConfigEntry<bool> ApplyDenyNameList { get; private set; }
    public static ConfigEntry<bool> ApplyBanList { get; private set; }
    public static ConfigEntry<bool> UnlockFPS { get; private set; }
    public static ConfigEntry<string> ChangeOutfit { get; private set; }
    public static ConfigEntry<bool> AutoStartGame { get; private set; }
    public static ConfigEntry<bool> AutoEndGame { get; private set; }
    public static ConfigEntry<bool> EnableMapBackGround { get; private set; }
    public static ConfigEntry<bool> EnableRoleBackGround { get; private set; }
    public static ConfigEntry<bool> DisableVanillaSound { get; private set; }

    public static ConfigEntry<bool> VersionCheat { get; private set; }
    public static ConfigEntry<bool> GodMode { get; private set; }



    public static readonly string[] changeOutfit =
    {
        "BeanMode", "HorseMode", "LongMode"
    };
    //Other Configs
    public static ConfigEntry<string> HideName { get; private set; }
    public static ConfigEntry<string> HideColor { get; private set; }
    public static ConfigEntry<bool> ShowResults { get; private set; }
    public static ConfigEntry<string> WebhookURL { get; private set; }


    public static Dictionary<RoleTypes, string> roleColors;
    public static List<int> clientIdList = new();

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
        new() { "冰激凌", "奶茶", "巧克力", "蛋糕", "甜甜圈", "可乐", "柠檬水", "冰糖葫芦", "果冻", "糖果", "牛奶", 
            "抹茶", "烧仙草", "菠萝包", "布丁", "椰子冻", "曲奇", "红豆土司", "三彩团子", "艾草团子", "泡芙", "可丽饼",
            "桃酥", "麻薯", "鸡蛋仔", "马卡龙", "雪梅娘", "炒酸奶", "蛋挞", "松饼", "西米露", "奶冻", "奶酥", "可颂", "奶糖" };
    public static List<string> TName_Snacks_EN = new() 
    { "Ice cream", "Milk tea", "Chocolate", "Cake", "Donut", "Coke", "Lemonade", "Candied haws", "Jelly", "Candy", "Milk", 
        "Matcha", "Burning Grass Jelly", "Pineapple Bun", "Pudding", "Coconut Jelly", "Cookies", "Red Bean Toast", 
        "Three Color Dumplings", "Wormwood Dumplings", "Puffs", "Can be Crepe", "Peach Crisp", "Mochi", "Egg Waffle", "Macaron",
        "Snow Plum Niang", "Fried Yogurt", "Egg Tart", "Muffin", "Sago Dew", "panna cotta", "soufflé", "croissant", "toffee" };
    public static string Get_TName_Snacks => TranslationController.Instance.currentLanguage.languageID is SupportedLangs.SChinese or SupportedLangs.TChinese ?
        TName_Snacks_CN[IRandom.Instance.Next(0, TName_Snacks_CN.Count)] :
        TName_Snacks_EN[IRandom.Instance.Next(0, TName_Snacks_EN.Count)];

    public override void Load()
    {
        Instance = this;

        //Client Options
        HideName = Config.Bind("Client Options", "Hide Game Code Name", "Final Suspect");
        HideColor = Config.Bind("Client Options", "Hide Game Code Color", $"{ColorHelper.ModColor}");
        DebugKeyInput = Config.Bind("Authentication", "Debug Key", "");
        ShowResults = Config.Bind("Result", "Show Results", true);
        UnlockFPS = Config.Bind("Client Options", "UnlockFPS", false);
        ChangeOutfit = Config.Bind("Client Options", "AprilFoolsMode", changeOutfit[0]);
        KickPlayerFriendCodeNotExist = Config.Bind("Client Options", "KickPlayerFriendCodeNotExist", true);
        ApplyBanList = Config.Bind("Client Options", "ApplyBanList", true);
        ApplyDenyNameList = Config.Bind("Client Options", "ApplyDenyNameList", true);
        AutoStartGame = Config.Bind("Client Options", "AutoStartGame", false);
        AutoEndGame = Config.Bind("Client Options", "AutoEndGame", false);
        EnableMapBackGround = Config.Bind("Client Options", "EnableMapBackGround", true);
        EnableRoleBackGround = Config.Bind("Client Options", "EnableRoleBackGround", true);
        DisableVanillaSound = Config.Bind("Client Options", "DisableVanillaSound", false);
        VersionCheat = Config.Bind("Client Options", "VersionCheat", false);
        GodMode = Config.Bind("Client Options", "GodMode", false);

        Logger = BepInEx.Logging.Logger.CreateLogSource("FinalSuspect");
        FinalSuspect.Logger.Enable();
        FinalSuspect.Logger.Disable("SwitchSystem");
        FinalSuspect.Logger.Disable("ModNews");
        FinalSuspect.Logger.Disable("CancelPet");
        if (!DebugModeManager.AmDebugger)
        {
            FinalSuspect.Logger.Disable("test");
        }
        //FinalSuspect.Logger.isDetail = true;

        // 認証関連-初期化
        DebugKeyAuth = new HashAuth(DebugKeyHash, DebugKeySalt);

        // 認証関連-認証
        DebugModeManager.Auth(DebugKeyAuth, DebugKeyInput.Value);

        WebhookURL = Config.Bind("Other", "WebhookURL", "none");

        hasArgumentException = false;
        ExceptionMessage = "";
        try
        {
            roleColors = new Dictionary<RoleTypes, string>()
            {
                {RoleTypes.CrewmateGhost, "#8CFFFF"},
                {RoleTypes.GuardianAngel, "#8CFFDB"},
                {RoleTypes.Crewmate, "#8CFFFF"},
                {RoleTypes.Scientist, "#F8FF8C"},
                {RoleTypes.Engineer, "#A5A8FF"},
                {RoleTypes.Noisemaker, "#FFC08C"},
                {RoleTypes.Tracker, "#93FF8C"},
                {RoleTypes.ImpostorGhost, "#FF1919"},
                {RoleTypes.Impostor, "#FF1919"},
                {RoleTypes.Shapeshifter, "#FF819E"},
                {RoleTypes.Phantom, "#CA8AFF"},
            };
        }
        catch (ArgumentException ex)
        {
            FinalSuspect.Logger.Error("错误：字典出现重复项", "LoadDictionary");
            FinalSuspect.Logger.Exception(ex, "LoadDictionary");
            hasArgumentException = true;
            ExceptionMessage = ex.Message;
            ExceptionMessageIsShown = false;
        }

        RegistryManager.Init(); // 这是优先级最高的模块初始化方法，不能使用模块初始化属性

        PluginModuleInitializerAttribute.InitializeAll();

        IRandom.SetInstance(new NetRandomWrapper());

        FinalSuspect.Logger.Info($"{Application.version}", "AmongUs Version");

        var handler = FinalSuspect.Logger.Handler("GitVersion");
        handler.Info($"{nameof(ThisAssembly.Git.BaseTag)}: {ThisAssembly.Git.BaseTag}");
        handler.Info($"{nameof(ThisAssembly.Git.Commit)}: {ThisAssembly.Git.Commit}");
        handler.Info($"{nameof(ThisAssembly.Git.Commits)}: {ThisAssembly.Git.Commits}");
        handler.Info($"{nameof(ThisAssembly.Git.IsDirty)}: {ThisAssembly.Git.IsDirty}");
        handler.Info($"{nameof(ThisAssembly.Git.Sha)}: {ThisAssembly.Git.Sha}");
        handler.Info($"{nameof(ThisAssembly.Git.Tag)}: {ThisAssembly.Git.Tag}");

        ClassInjector.RegisterTypeInIl2Cpp<ErrorText>();

        SystemEnvironment.SetEnvironmentVariables();

        Harmony.PatchAll();

        if (DebugModeManager.AmDebugger) ConsoleManager.CreateConsole();
        else ConsoleManager.DetachConsole();

        FinalSuspect.Logger.Msg("========= FinalSuspect loaded! =========", "Plugin Load");
    }
}
