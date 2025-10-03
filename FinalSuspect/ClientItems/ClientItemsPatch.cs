using System;
using System.IO;
using BepInEx.Configuration;
using FinalSuspect.ClientItems.FeatureItems;
using FinalSuspect.ClientItems.FeatureItems.MainMenuStyle;
using FinalSuspect.ClientItems.FeatureItems.MyMusic;
using FinalSuspect.ClientItems.FeatureItems.NameTag;
using FinalSuspect.ClientItems.FeatureItems.Resources;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Features;
using FinalSuspect.Patches.System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FinalSuspect.ClientItems;

[HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
public static class OptionsMenuBehaviourStartPatch
{
    private static ClientOptionItem<bool> _unlockFPS;
    private static ClientOptionItem<OutfitType> _switchOutfitType;
    private static ClientOptionItem<bool> _kickPlayerWithAbnormalFriendCode;
    private static ClientOptionItem<bool> _kickPlayerWithDenyName;
    private static ClientOptionItem<bool> _kickPlayerInBanList;
    private static ClientOptionItem<bool> _spamDenyWord;
    private static ClientOptionItem<bool> _autoStartGame;
    private static ClientOptionItem<bool> _autoEndGame;

    private static ClientOptionItem<bool> _disableVanillaSound;
    private static ClientOptionItem<bool> _enableFac;
    private static ClientOptionItem<bool> _enableGuardian;
    private static ClientOptionItem<bool> _showPlayerInfo;
    private static ClientOptionItem<bool> _useModCursor;
    private static ClientOptionItem<bool> _fastLaunchMode;
    private static ClientOptionItem<bool> _offlineMode;

    private static ClientOptionItem<bool> _versionCheat;
    private static ClientOptionItem<bool> _godMode;
    private static ClientOptionItem<bool> _noGameEnd;

    private static ClientFeatureItem _clearAutoLogs;
    private static ClientFeatureItem _dumpLog;
    private static ClientFeatureItem _unloadMod;
    private static ClientFeatureItem _mainMenuStyleBtn;

    private static ClientFeatureItem _resourceBtn;

    private static ClientFeatureItem _myMusicBtn;
    private static ClientFeatureItem _nameTagBtn;


    private static bool _reseted;
    public static bool Recreate;
    public static OptionsMenuBehaviour Instance { get; private set; }

    public static void Postfix(OptionsMenuBehaviour __instance)
    {
        if (!__instance.DisableMouseMovement) return;
        Instance = __instance;

        if (!_reseted || !DebugModeManager.IsDebugMode)
        {
            _reseted = true;
            ConfigManager.VersionCheat.Value = false;
            ConfigManager.GodMode.Value = false;
            ConfigManager.NoGameEnd.Value = false;
        }

        if (Recreate)
        {
            ClientActionItem.ModOptionsButton.gameObject.SetActive(false);
            Object.Destroy(ClientActionItem.ModOptionsButton);
            Object.Destroy(ClientActionItem.CustomBackground);
            ClientFeatureItem.ModOptionsButton.gameObject.SetActive(false);
            Object.Destroy(ClientFeatureItem.ModOptionsButton);
            Object.Destroy(ClientFeatureItem.CustomBackground);

            Object.Destroy(ModUnloaderScreen.Popup);
            Object.Destroy(MainMenuStylePanel.CustomBackground);
            Object.Destroy(ResourcesPanel.CustomBackground);
            Object.Destroy(MyMusicPanel.CustomBackground);
            Object.Destroy(NameTagPanel.CustomBackground);

            ClientActionItem.ModOptionsButton = null;
            ClientActionItem.CustomBackground = null;

            ClientFeatureItem.ModOptionsButton = null;
            ClientFeatureItem.CustomBackground = null;

            ModUnloaderScreen.Popup = null;
            MainMenuStylePanel.CustomBackground = null;
            ResourcesPanel.CustomBackground = null;
            MyMusicPanel.CustomBackground = null;
            NameTagPanel.CustomBackground = null;
        }

        CreateOptionItem(ref _unlockFPS, "UnlockFPS", ConfigManager.UnlockFPS, __instance, UnlockFPSButtonToggle);
        CreateOptionItem(ref _switchOutfitType, "SwitchOutfitType", ConfigManager.SwitchOutfitType, __instance,
            SwitchMode);
        CreateOptionItem(ref _kickPlayerWithAbnormalFriendCode, "KickPlayerWithAbnormalFriendCode",
            ConfigManager.KickPlayerWithAbnormalFriendCode, __instance);
        CreateOptionItem(ref _kickPlayerInBanList, "KickPlayerInBanList", ConfigManager.KickPlayerInBanList,
            __instance);
        CreateOptionItem(ref _kickPlayerWithDenyName, "KickPlayerWithDenyName", ConfigManager.KickPlayerWithDenyName,
            __instance);
        CreateOptionItem(ref _spamDenyWord, "SpamDenyWord", ConfigManager.SpamDenyWord, __instance);
        CreateOptionItem(ref _enableFac, "EnableFAC", ConfigManager.EnableFAC, __instance);
        CreateOptionItem(ref _enableGuardian, "EnableGuardian", ConfigManager.EnableGuardian, __instance);
        CreateOptionItem(ref _autoStartGame, "AutoStartGame", ConfigManager.AutoStartGame, __instance,
            AutoStartButtonToggle);
        CreateOptionItem(ref _autoEndGame, "AutoEndGame", ConfigManager.AutoEndGame, __instance);
        //CreateOptionItem<bool>(ref PrunkMode, "PrunkMode", Main.PrunkMode, __instance);
        CreateOptionItem(ref _disableVanillaSound, "DisableVanillaSound", ConfigManager.DisableVanillaSound, __instance,
            () =>
            {
                if (ConfigManager.DisableVanillaSound.Value)
                    AudioPlayer.StopPlayVanilla();
                else
                    AudioPlayer.StartPlayVanilla();
            });
        CreateOptionItem(ref _showPlayerInfo, "ShowPlayerInfo", ConfigManager.ShowPlayerInfo, __instance);
        CreateOptionItem(ref _fastLaunchMode, "FastLaunchMode", ConfigManager.FastLaunchMode, __instance);
        CreateOptionItem(ref _offlineMode, "OfflineMode", ConfigManager.OfflineMode, __instance, (() =>
        {
            __instance.Close();
            CustomPopup.Show(GetString("ClientOption.OfflineMode"), GetString("UpdateResult.Succeed_Text"),
                [(GetString(StringNames.ExitGame), Application.Quit)]);
        }));
        CreateOptionItem(ref _useModCursor, "UseModCursor", ConfigManager.UseModCursor, __instance, SetCursor);

        if (DebugModeManager.IsDebugMode)
        {
            CreateOptionItem(ref _versionCheat, "VersionCheat", ConfigManager.VersionCheat, __instance);
            CreateOptionItem(ref _godMode, "GodMode", ConfigManager.GodMode, __instance);
            CreateOptionItem(ref _noGameEnd, "NoGameEnd", ConfigManager.NoGameEnd, __instance);
        }

        CreateFeatureItem(ref _dumpLog, "DumpLog", () => { DumpLog(); }, __instance);
        CreateFeatureItem(ref _clearAutoLogs, "ClearAutoLogs", () =>
        {
            ClearAutoLogs();
            SetFeatureItemDisabled(_clearAutoLogs);
        }, __instance);
        CreateFeatureItem(ref _unloadMod, "UnloadMod", ModUnloaderScreen.Show, __instance);


        CreateFeatureItem(ref _mainMenuStyleBtn, "MainMenuStyleManager",
            () => { MainMenuStylePanel.CustomBackground?.gameObject.SetActive(true); }, __instance);
        CreateFeatureItem(ref _resourceBtn, "ResourceManager",
            () => { ResourcesPanel.CustomBackground?.gameObject.SetActive(true); }, __instance);
        CreateFeatureItem(ref _myMusicBtn, "SoundOption",
            () => { MyMusicPanel.CustomBackground?.gameObject.SetActive(true); }, __instance);
        CreateFeatureItem(ref _nameTagBtn, "NameTagManager",
            () => { NameTagPanel.CustomBackground?.gameObject.SetActive(true); }, __instance);

        SetFeatureItemTextAndColor(_mainMenuStyleBtn, "MainMenuStyleManager");
        SetFeatureItemTextAndColor(_resourceBtn, "ResourceManager");
        SetFeatureItemTextAndColor(_myMusicBtn, "MyMusic");
        SetFeatureItemTextAndColor(_nameTagBtn, "NameTagManager");

        if (!IsNotJoined)
        {
            SetFeatureItemDisabled_Menu(_resourceBtn);
            SetFeatureItemDisabled_Menu(_mainMenuStyleBtn);
            SetOptionItemDisabled(_offlineMode);
        }

        if (Directory.GetFiles(GetLogFolder(true).FullName).Length <= 0)
            SetFeatureItemDisabled(_clearAutoLogs);

        MainMenuStylePanel.Init(__instance);
        ResourcesPanel.Init(__instance);
        MyMusicPanel.Init(__instance);
        NameTagPanel.Init(__instance);


        if (!ModUnloaderScreen.Popup)
            ModUnloaderScreen.Init(__instance);
        Recreate = false;
    }

    private static void CreateOptionItem<T>(ref ClientOptionItem<T> item, string name, ConfigEntry<T> value,
        OptionsMenuBehaviour instance, Action toggleAction = null)
    {
        if (Recreate)
        {
            Object.Destroy(item.ToggleButton.gameObject);
            item = null;
        }

        if (item == null || !item.ToggleButton) item = ClientOptionItem<T>.Create(name, value, instance, toggleAction);
    }

    /*private static void CreateActionItem(ref ClientActionItem item, string name, Action action, OptionsMenuBehaviour instance)
    {
        if (recreate)
        {
            Object.Destroy(item.ToggleButton.gameObject);
            item = null;
        }

        if (item == null || !item.ToggleButton)
        {
            item = ClientActionItem.Create(name, action, instance);
        }
    }*/

    private static void CreateFeatureItem(ref ClientFeatureItem item, string name, Action action,
        OptionsMenuBehaviour instance)
    {
        if (Recreate)
        {
            Object.Destroy(item.ToggleButton.gameObject);
            item = null;
        }

        if (item == null || !item.ToggleButton) item = ClientFeatureItem.Create(name, action, instance);
    }

    private static void SetFeatureItemTextAndColor(ClientFeatureItem item, string text)
    {
        item.ToggleButton.Text.text = GetString("ClientFeature." + text);
        item.ToggleButton.GetComponent<PassiveButton>().enabled = true;
        item.ToggleButton.Background.color = ColorHelper.FSClientFeatureColor;
    }

    /*private static void SetOptionItemDisabled(ClientOptionItem_Boolean item)
    {
        item.ToggleButton.Text.text += $"\n|{GetString("OnlyAvailableInMainMenu")}|";
        item.ToggleButton.GetComponent<PassiveButton>().enabled = false;
        item.ToggleButton.Background.color = ColorHelper.ClientOptionColor_CanNotUse;
    }*/

    private static void SetOptionItemDisabled<T>(ClientOptionItem<T> item)
    {
        item.ToggleButton.Text.text += $"\n|{GetString("Tip.OnlyAvailableInMainMenu")}|";
        item.ToggleButton.GetComponent<PassiveButton>().enabled = false;
        item.ToggleButton.Background.color = ColorHelper.FSClientOptionColor_CanNotUse;
    }

    private static void SetFeatureItemDisabled_Menu(ClientFeatureItem item)
    {
        item.ToggleButton.Text.text += $"\n|{GetString("Tip.OnlyAvailableInMainMenu")}|";
        SetFeatureItemDisabled(item);
    }

    private static void SetFeatureItemDisabled(ClientFeatureItem item)
    {
        item.ToggleButton.GetComponent<PassiveButton>().enabled = false;
        item.ToggleButton.Background.color = ColorHelper.FSClientFeatureColor_CanNotUse;
    }

    /*private static void SetFeatureItemEnable(ClientFeatureItem item)
    {
        item.ToggleButton.GetComponent<PassiveButton>().enabled = true;
        item.ToggleButton.Background.color = ColorHelper.ClientFeatureColor;
    }*/

    private static void UnlockFPSButtonToggle()
    {
        Application.targetFrameRate = ConfigManager.UnlockFPS.Value ? 165 : 60;
        SendInGame(string.Format(GetString("Notification.FPSSetTo"), Application.targetFrameRate));
    }

    private static void SwitchMode()
    {
        foreach (var pc in Main.AllPlayerControls)
        {
            pc.MyPhysics.SetBodyType(pc.BodyType);
            if (pc.BodyType == PlayerBodyTypes.Normal)
                pc.cosmetics.currentBodySprite.BodySprite.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        }
    }

    private static void AutoStartButtonToggle()
    {
        if (!ConfigManager.AutoStartGame.Value && IsCountDown) GameStartManager.Instance.ResetStartState();
    }

    public static void SetCursor()
    {
        try
        {
            var sprite = LoadSprite("Cursor.png");
            Cursor.SetCursor(ConfigManager.UseModCursor.Value ? sprite.texture : null, Vector2.zero, CursorMode.Auto);
        }
        catch
        {
            ConfigManager.UseModCursor.Value = false;
        }
    }
}

[HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Close))]
public static class OptionsMenuBehaviourClosePatch
{
    public static void Postfix()
    {
        ClientActionItem.CustomBackground?.gameObject.SetActive(false);
        ClientFeatureItem.CustomBackground?.gameObject.SetActive(false);
        ModUnloaderScreen.Hide();
        MainMenuStylePanel.Hide();
        ResourcesPanel.Hide();
        MyMusicPanel.Hide();
        NameTagPanel.Hide();
    }
}

[HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Update))]
public static class OptionsMenuBehaviourUpdatePatch
{
    public static void Postfix()
    {
        MyMusicPanel.Update();
    }
}

[HarmonyPatch(typeof(LanguageSetter), nameof(LanguageSetter.SetLanguage))]
public static class LanguageSetterSetLanguagePatch
{
    public static void Postfix()
    {
        OptionsMenuBehaviourStartPatch.Recreate = true;
        try
        {
            Object.Destroy(ModMainMenuManager.VisitText);
        }
        catch
        {
            /* ignored */
        }

        ModMainMenuManager.VisitText = null;
        VersionShowerStartPatch.CreateVisitText(null);
        OptionsMenuBehaviourStartPatch.Postfix(OptionsMenuBehaviourStartPatch.Instance);
    }
}