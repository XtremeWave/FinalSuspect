using System;
using BepInEx.Configuration;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.ClientOptions;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Modules.SoundInterface;
using UnityEngine;
using Object = System.Object;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
public static class OptionsMenuBehaviourStartPatch
{
    private static ClientOptionItem_Boolean UnlockFPS;
    private static ClientOptionItem_String ChangeOutfit;
    private static ClientOptionItem_Boolean KickPlayerFriendCodeNotExist;
    private static ClientOptionItem_Boolean KickPlayerWithDenyName;
    private static ClientOptionItem_Boolean KickPlayerInBanList;
    private static ClientOptionItem_Boolean SpamDenyWord;
    private static ClientOptionItem_Boolean AutoStartGame;
    private static ClientOptionItem_Boolean AutoEndGame;
    //private static ClientOptionItem_Boolean PrunkMode;
    private static ClientOptionItem_Boolean DisableVanillaSound;
    private static ClientOptionItem_Boolean DisableFAC;
    private static ClientOptionItem_Boolean ShowPlayerInfo;
    private static ClientOptionItem_Boolean UseModCursor;
    private static ClientOptionItem_Boolean FastBoot;
    private static ClientFeatureItem UnloadMod;
    private static ClientFeatureItem DumpLog;
    private static ClientOptionItem_Boolean VersionCheat;
    private static ClientOptionItem_Boolean GodMode;
    private static ClientOptionItem_Boolean NoGameEnd;

    public static ClientFeatureItem SoundBtn;
    public static ClientFeatureItem AudioManagementBtn;
    public static OptionsMenuBehaviour Instance { get; private set; }
    private static bool reseted;
    public static bool recreate;

    public static void Postfix(OptionsMenuBehaviour __instance)
    {
        if (__instance.DisableMouseMovement == null) return;
        Instance = __instance;

        if (!reseted || !DebugModeManager.AmDebugger)
        {
            reseted = true;
            Main.VersionCheat.Value = false;
            Main.GodMode.Value = false;
            Main.NoGameEnd.Value = false;
        }

        if (recreate)
        {
            ClientActionItem.ModOptionsButton.gameObject.SetActive(false);
            GameObject.Destroy(ClientActionItem.ModOptionsButton);
            GameObject.Destroy(ClientActionItem.CustomBackground);
            ClientFeatureItem.ModOptionsButton.gameObject.SetActive(false);
            GameObject.Destroy(ClientFeatureItem.ModOptionsButton);
            GameObject.Destroy(ClientFeatureItem.CustomBackground);

            GameObject.Destroy(ModUnloaderScreen.Popup);
            GameObject.Destroy(MyMusicPanel.CustomBackground);
            GameObject.Destroy(SoundManagementPanel.CustomBackground);
            ClientActionItem.ModOptionsButton = null;
            ClientActionItem.CustomBackground = null;

            ClientFeatureItem.ModOptionsButton = null;
            ClientFeatureItem.CustomBackground = null;

            ModUnloaderScreen.Popup = null;
            MyMusicPanel.CustomBackground = null;
            SoundManagementPanel.CustomBackground = null;
        }
        CreateOptionItem(ref UnlockFPS, "UnlockFPS", Main.UnlockFPS, __instance, UnlockFPSButtonToggle);
        CreateOptionItem(ref ChangeOutfit, "ChangeOutfit", Main.ChangeOutfit, __instance, Main.OutfitType, SwitchHorseMode);
        CreateOptionItem(ref KickPlayerFriendCodeNotExist, "KickPlayerFriendCodeNotExist", Main.KickPlayerWhoFriendCodeNotExist, __instance);
        CreateOptionItem(ref KickPlayerInBanList, "KickPlayerInBanList", Main.KickPlayerInBanList, __instance);
        CreateOptionItem(ref KickPlayerWithDenyName, "KickPlayerWithDenyName", Main.KickPlayerWithDenyName, __instance);
        CreateOptionItem(ref SpamDenyWord, "SpamDenyWord", Main.SpamDenyWord, __instance);
        CreateOptionItem(ref AutoStartGame, "AutoStartGame", Main.AutoStartGame, __instance, AutoStartButtonToggle);
        CreateOptionItem(ref AutoEndGame, "AutoEndGame", Main.AutoEndGame, __instance);
        //CreateOptionItem(ref PrunkMode, "PrunkMode", Main.PrunkMode, __instance);
        CreateOptionItem(ref DisableVanillaSound, "DisableVanillaSound", Main.DisableVanillaSound, __instance, () => {
            if (Main.DisableVanillaSound.Value)
                CustomSoundsManager.StopPlayVanilla();
            else
            {
                CustomSoundsManager.StartPlayVanilla();
            }
        });
        CreateOptionItem(ref DisableFAC, "DisableFAC", Main.DisableFAC, __instance);
        CreateOptionItem(ref ShowPlayerInfo, "ShowPlayerInfo", Main.ShowPlayerInfo, __instance);
        CreateOptionItem(ref UseModCursor, "UseModCursor", Main.UseModCursor, __instance, SetCursor);
        CreateOptionItem(ref FastBoot, "FastBoot", Main.FastBoot, __instance);
        if (DebugModeManager.AmDebugger)
        {
            CreateOptionItem(ref VersionCheat, "VersionCheat", Main.VersionCheat, __instance);
            CreateOptionItem(ref GodMode, "GodMode", Main.GodMode, __instance);
            CreateOptionItem(ref NoGameEnd, "NoGameEnd", Main.NoGameEnd, __instance);
        }

        CreateFeatureItem(ref UnloadMod, "UnloadMod", ModUnloaderScreen.Show, __instance);
        CreateFeatureItem(ref DumpLog, "DumpLog", () => Utils.DumpLog(), __instance);
        CreateFeatureItem(ref SoundBtn, "SoundOption", () =>
        {
            MyMusicPanel.CustomBackground?.gameObject?.SetActive(true);
        }, __instance);
        CreateFeatureItem(ref AudioManagementBtn, "SoundManager", () =>
        {
            SoundManagementPanel.CustomBackground?.gameObject?.SetActive(true);
        }, __instance);

        SetFeatureItemTextAndColor(SoundBtn, "SoundOptions");
        SetFeatureItemTextAndColor(AudioManagementBtn, "AudioManagementOptions");

        if (!XtremeGameData.GameStates.IsNotJoined)
        {
            SetOptionItemDisabled(ChangeOutfit);
            SetFeatureItemDisabled(AudioManagementBtn);
        }

        MyMusicPanel.Init(__instance);
        SoundManagementPanel.Init(__instance);

        
        if (ModUnloaderScreen.Popup == null)
            ModUnloaderScreen.Init(__instance);
        recreate = false;
    }

    private static void CreateOptionItem(ref ClientOptionItem_Boolean item, string name, ConfigEntry<bool> value, OptionsMenuBehaviour instance, Action toggleAction = null)
    {
        if (recreate)
        {
            GameObject.Destroy(item.ToggleButton.gameObject);
            item = null;
        }
        if (item == null || item.ToggleButton == null)
        {
            item = ClientOptionItem_Boolean.Create(name, value, instance, toggleAction);
        }

        
    }

    private static void CreateOptionItem(ref ClientOptionItem_String item, string name, ConfigEntry<string> value, OptionsMenuBehaviour instance, string[] options, Action toggleAction = null)
    {
        if (recreate)
        {
            GameObject.Destroy(item.ToggleButton.gameObject);
            item = null;
        }
        if (item == null || item.ToggleButton == null)
        {
            item = ClientOptionItem_String.Create(name,value.Value ?? options[0], value, instance, options, toggleAction);
        }
    }

    private static void CreateActionItem(ref ClientActionItem item, string name, Action action, OptionsMenuBehaviour instance)
    {
        if (recreate)
        {
            GameObject.Destroy(item.ToggleButton.gameObject);
            item = null;
        }
        if (item == null || item.ToggleButton == null)
        {
            item = ClientActionItem.Create(name, action, instance);
        }
    }

    private static void CreateFeatureItem(ref ClientFeatureItem item, string name, Action action, OptionsMenuBehaviour instance)
    {
        if (recreate)
        {
            GameObject.Destroy(item.ToggleButton.gameObject);
            item = null;
        }
        if (item == null || item.ToggleButton == null)
        {
            item = ClientFeatureItem.Create(name, action, instance);
        }
    }

    private static void SetFeatureItemTextAndColor(ClientFeatureItem item, string text)
    {
        item.ToggleButton.Text.text = GetString(text);
        item.ToggleButton.GetComponent<PassiveButton>().enabled = true;
        item.ToggleButton.Background.color = ColorHelper.ClientFeatureColor;
    }
    private static void SetOptionItemDisabled(ClientOptionItem_Boolean item)
    {
        item.ToggleButton.Text.text += $"\n|{GetString("OnlyAvailableInMainMenu")}|";
        item.ToggleButton.GetComponent<PassiveButton>().enabled = false;
        item.ToggleButton.Background.color = ColorHelper.ClientOptionColor_CanNotUse;
    }
    private static void SetOptionItemDisabled(ClientOptionItem_String item)
    {
        item.ToggleButton.Text.text = GetString(item.Name) + $"\n|{GetString("OnlyAvailableInMainMenu")}|";
        item.ToggleButton.GetComponent<PassiveButton>().enabled = false;
        item.ToggleButton.Background.color = ColorHelper.ClientOptionColor_CanNotUse;
    }
    private static void SetFeatureItemDisabled(ClientFeatureItem item)
    {
        item.ToggleButton.Text.text += $"\n|{GetString("OnlyAvailableInMainMenu")}|";
        item.ToggleButton.GetComponent<PassiveButton>().enabled = false;
        item.ToggleButton.Background.color = ColorHelper.ClientFeatureColor_CanNotUse;
    }

    private static void UnlockFPSButtonToggle()
    {
        Application.targetFrameRate = Main.UnlockFPS.Value ? 165 : 60;
        XtremeLogger.SendInGame(string.Format(GetString("FPSSetTo"), Application.targetFrameRate));
    }

    private static void SwitchHorseMode()
    {
        ChangeOutfit.UpdateToggle(Main.OutfitType);
        //if (Main.ChangeOutfit.Value == Main.changeOutfit[1])
        //foreach (var pc in PlayerControl.AllPlayerControls)
        //{
        //    pc.MyPhysics.SetBodyType(pc.BodyType);
        //    if (pc.BodyType == PlayerBodyTypes.Normal)
        //    {
        //        pc.cosmetics.currentBodySprite.BodySprite.transform.localScale = new(0.5f, 0.5f, 1f);
        //    }
        //}
    }

    private static void AutoStartButtonToggle()
    {
        if (Main.AutoStartGame.Value == false && XtremeGameData.GameStates.IsCountDown)
        {
            GameStartManager.Instance.ResetStartState();
        }
    }
    
    public static void SetCursor()
    {
        try
        {
            var sprite = Utils.LoadSprite("Cursor.png");
            Cursor.SetCursor(Main.UseModCursor.Value ? sprite.texture: null, Vector2.zero, CursorMode.Auto);
        }
        catch
        {
            Main.UseModCursor.Value = false;
        }
       
    }
}
[HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Close))]
public static class OptionsMenuBehaviourClosePatch
{
    public static void Postfix()
    {
        ClientActionItem.CustomBackground?.gameObject?.SetActive(false);
        ClientFeatureItem.CustomBackground?.gameObject?.SetActive(false);
        ModUnloaderScreen.Hide();
        MyMusicPanel.Hide();
        SoundManagementPanel.Hide();
    }
}

[HarmonyPatch(typeof(LanguageSetter), nameof(LanguageSetter.SetLanguage))]
public static class LanguageSetterSetLanguagePatch
{
    public static void Postfix()
    {
        OptionsMenuBehaviourStartPatch.recreate = true;
        try
        {
            GameObject.Destroy(VersionShowerStartPatch.VisitText);
        }
        catch 
        {
        }
        VersionShowerStartPatch.VisitText = null;
        VersionShowerStartPatch.CreateVisitText(null);
        OptionsMenuBehaviourStartPatch.Postfix(OptionsMenuBehaviourStartPatch.Instance);
    }
}