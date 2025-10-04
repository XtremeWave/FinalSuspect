using System;
using AmongUs.GameOptions;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using FinalSuspect;
using FinalSuspect.Attributes;
using FinalSuspect.Internal;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using FinalSuspect.Modules.Resources;
using Il2CppInterop.Runtime.Injection;

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
    public const string PluginVersion = "1.2.99";
    public const string PluginGuid = "cn.slok.finalsuspect";
    public const int PluginCreation = 1;

    // == 版本相关设定 / Version Config ==
    public const string LowestSupportedVersion = "2025.9.9"; // 17.0.0

    private const string DisplayedVersion_Head = "1.3";

    private const string DisplayedVersion_Date = BuildTime.Date;

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

    public static readonly bool IsInitialRelease = DateTime.Now is { Month: 8, Day: >= 15 and <= 19 };
    public static readonly bool IsAprilFools = DateTime.Now is { Month: 4, Day: >= 1 and <= 10 };

    public static readonly bool IsValentines = DateTime.Now is { Month: 2, Day: >= 14 and <= 20 } ||
                                               DateTime.Now.Year is 2025 && DateTime.Now.Month is 8 &&
                                               DateTime.Now.Day is 29;

    public static Main Instance;

    // ==========
    public Harmony Harmony { get; } = new(PluginGuid);
    public static NormalGameOptionsV10 NormalOptions => GameOptionsManager.Instance.currentNormalGameOptions;
    public static HideNSeekGameOptionsV10 HideNSeekOptions => GameOptionsManager.Instance.currentHideNSeekGameOptions;

    public static IEnumerable<PlayerControl> AllPlayerControls =>
        PlayerControl.AllPlayerControls.ToArray().Where(p => p);

    public static IEnumerable<PlayerControl> AllAlivePlayerControls =>
        PlayerControl.AllPlayerControls.ToArray().Where(p => p && p.IsAlive() && !p.Data.Disconnected);

    public override void Load()
    {
        Instance = this;

        hasArgumentException = false;
        ExceptionMessage = "";
        PluginModuleInitializerAttribute.InitializeAll();

        ClassInjector.RegisterTypeInIl2Cpp<ErrorText>();
        Harmony.PatchAll();


        if (DebugModeManager.IsDebugMode) ConsoleManager.CreateConsole();
        else ConsoleManager.DetachConsole();

        Msg("========= FinalSuspect loaded! =========", "Plugin Load");
        Application.quitting += new Action(() => VersionChecker.CancellationToken.Cancel());
        Application.quitting += new Action(SaveNowLog);
    }

#if !RELEASE
    /// <summary>
    ///     表示当前显示的版本类型。
    /// </summary>
    private const VersionTypes DisplayedVersion_Type = VersionTypes.Canary;

    private const int DisplayedVersion_TestCreation = 1;
#endif
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