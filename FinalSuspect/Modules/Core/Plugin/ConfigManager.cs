using BepInEx.Configuration;
using FinalSuspect.Attributes;
using FinalSuspect.Helpers;

namespace FinalSuspect.Modules.Core.Plugin;

public static class ConfigManager
{
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
#if Windows
    public static ConfigEntry<bool> UseModCursor { get; private set; }
#endif
    public static ConfigEntry<bool> VersionCheat { get; private set; }
    public static ConfigEntry<bool> GodMode { get; private set; }
    public static ConfigEntry<bool> NoGameEnd { get; private set; }
    public static ConfigEntry<string> HideName { get; private set; }
    public static ConfigEntry<string> HideColor { get; private set; }
    public static ConfigEntry<bool> ShowResults { get; private set; }
    public static ConfigEntry<bool> ShowInfoPanel { get; private set; }
    public static ConfigEntry<bool> EnableFinalSuspect { get; private set; }
    public static ConfigEntry<BypassType> LanguageUpdateBypass { get; private set; }
    public static ConfigEntry<int> CurrentStyleId { get; private set; }

    [PluginModuleInitializer(InitializePriority.VeryHigh)]
    public static void OnInitialization()
    {
        var config = Main.Instance.Config;
        //Configs
        HideName = config.Bind("Final System", "Hide Game Code Name", "Final Suspect");
        HideColor = config.Bind("Final System", "Hide Game Code Color", $"{ColorHelper.FSColorHex}");
        EnableFinalSuspect = config.Bind("Final System", "Enable Final Suspect", true);
        ShowResults = config.Bind("Final System", "Show Results", true);
        ShowInfoPanel = config.Bind("Final System", "Show InfoPanel", true);
        LanguageUpdateBypass = config.Bind("Final System", "Language Update Bypass", BypassType.Dont);
        CurrentStyleId = config.Bind("Final System", "Background Id", 0);

        UnlockFPS = config.Bind("Client Options", "Unlock FPS", false);
        SwitchOutfitType = config.Bind("Client Options", "Switch Outfit", OutfitType.BeanMode);
        KickPlayerWithAbnormalFriendCode = config.Bind("Client Options", "Kick Player With Abnormal FriendCode", true);
        KickPlayerInBanList = config.Bind("Client Options", "Kick Player In Ban List", true);
        KickPlayerWithDenyName = config.Bind("Client Options", "Kick Player With Deny Name", true);
        SpamDenyWord = config.Bind("Client Options", "Spam Deny Word", true);
        AutoStartGame = config.Bind("Client Options", "Auto Start Game", false);
        AutoEndGame = config.Bind("Client Options", "Auto End Game", false);
        DisableVanillaSound = config.Bind("Client Options", "Disable Vanilla Sound", false);
        EnableFAC = config.Bind("Client Options", "Enable FAC", false);
        EnableGuardian = config.Bind("Client Options", "Enable Guardian", true);
        ShowPlayerInfo = config.Bind("Client Options", "Show Player Info", true);
        FastLaunchMode = config.Bind("Client Options", "Fast Launch Mode", false);
        OfflineMode = config.Bind("Client Options", "Offline Mode", false);
#if Windows
        UseModCursor = config.Bind("Client Options", "Use Mod Cursor", true);
#endif

        VersionCheat = config.Bind("Debug Options", "Version Cheat", false);
        GodMode = config.Bind("Debug Options", "God Mode", false);
        NoGameEnd = config.Bind("Debug Options", "No Game End", false);
    }
}