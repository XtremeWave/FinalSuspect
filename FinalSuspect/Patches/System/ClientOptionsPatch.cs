using HarmonyLib;
using FinalSuspect.Modules.ClientOptions;
using FinalSuspect.Modules.SoundInterface;
using UnityEngine;
using FinalSuspect.Modules.Managers;
using FinalSuspect.Player;
using Sentry.Unity.NativeUtils;

namespace FinalSuspect;

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
    private static ClientOptionItem_Boolean EnableMapBackGround;
    private static ClientOptionItem_Boolean EnableRoleBackGround;
    private static ClientOptionItem_Boolean PrunkMode;
    private static ClientOptionItem_Boolean DisableVanillaSound;
    private static ClientOptionItem_Boolean DisableFAC;
    private static ClientActionItem UnloadMod;
    private static ClientActionItem DumpLog;
    private static ClientOptionItem_Boolean VersionCheat;
    private static ClientOptionItem_Boolean GodMode;
    private static ClientOptionItem_Boolean NoGameEnd;

    public static ClientFeatureItem SoundBtn;
    public static ClientFeatureItem AudioManagementBtn;

    private static bool reseted = false;
    public static void Postfix(OptionsMenuBehaviour __instance)
    {
        if (__instance.DisableMouseMovement == null) return;

        

        if (!reseted || !DebugModeManager.AmDebugger)
        {
            reseted = true;
            Main.VersionCheat.Value = false;
            Main.GodMode.Value = false;
            Main.NoGameEnd.Value = false;
        }

      
        
        if (UnlockFPS == null || UnlockFPS.ToggleButton == null)
        {
            UnlockFPS = ClientOptionItem_Boolean.Create("UnlockFPS", Main.UnlockFPS, __instance, UnlockFPSButtonToggle);
            static void UnlockFPSButtonToggle()
            {
                Application.targetFrameRate = Main.UnlockFPS.Value ? 165 : 60;
                Logger.SendInGame(string.Format(Translator.GetString("FPSSetTo"), Application.targetFrameRate));
            }
        }
        if (ChangeOutfit == null || ChangeOutfit.ToggleButton == null)
        {
            ChangeOutfit = ClientOptionItem_String.Create(
                Main.ChangeOutfit.Value ?? Main.OutfitType[0] 
                 
                , Main.ChangeOutfit, __instance, Main.OutfitType, SwitchHorseMode);
            static void SwitchHorseMode()
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
            if (!XtremeGameData.GameStates.IsNotJoined)
            {
                ChangeOutfit.ToggleButton.GetComponent<PassiveButton>().enabled = false;
                ChangeOutfit.ToggleButton.Background.color = Palette.DisabledGrey;
                ChangeOutfit.ToggleButton.Text.text = Translator.GetString("ChangeOutfit") + "|" + Translator.GetString("OnlyAvailableInMainMenu");
            }
            else
            {
                ChangeOutfit.UpdateToggle(Main.OutfitType);
                ChangeOutfit.UpdateName(Main.ChangeOutfit.Value);
                ChangeOutfit.ToggleButton.GetComponent<PassiveButton>().enabled =true;
            }
        }
        
        if (KickPlayerFriendCodeNotExist== null || KickPlayerFriendCodeNotExist.ToggleButton == null)
        {
            KickPlayerFriendCodeNotExist = ClientOptionItem_Boolean.Create("KickPlayerFriendCodeNotExist", Main.KickPlayerWhoFriendCodeNotExist, __instance);
        }
        if (KickPlayerInBanList == null || KickPlayerInBanList.ToggleButton == null)
        {
            KickPlayerInBanList = ClientOptionItem_Boolean.Create("KickPlayerInBanList", Main.KickPlayerInBanList, __instance);
        }
        if (KickPlayerWithDenyName == null || KickPlayerWithDenyName.ToggleButton == null)
        {
            KickPlayerWithDenyName = ClientOptionItem_Boolean.Create("KickPlayerWithDenyName", Main.KickPlayerWithDenyName, __instance);
        }
        if (SpamDenyWord == null || SpamDenyWord.ToggleButton == null)
        {
            SpamDenyWord = ClientOptionItem_Boolean.Create("SpamDenyWord", Main.SpamDenyWord, __instance);
        }

        if (AutoStartGame == null || AutoStartGame.ToggleButton == null)
        {
            AutoStartGame = ClientOptionItem_Boolean.Create("AutoStartGame", Main.AutoStartGame, __instance, AutoStartButtonToggle);
            static void AutoStartButtonToggle()
            {
                if (Main.AutoStartGame.Value == false && XtremeGameData.GameStates.IsCountDown)
                {
                    GameStartManager.Instance.ResetStartState();
                }
            }
        }
        if (AutoEndGame == null || AutoEndGame.ToggleButton == null)
        {
            AutoEndGame = ClientOptionItem_Boolean.Create("AutoEndGame", Main.AutoEndGame, __instance);
        }
        if (EnableRoleBackGround == null || EnableRoleBackGround.ToggleButton == null)
        {
            EnableRoleBackGround = ClientOptionItem_Boolean.Create("EnableRoleBackGround", Main.EnableRoleBackGround, __instance);
        }
        if (EnableMapBackGround == null || EnableMapBackGround.ToggleButton == null)
        {
            EnableMapBackGround = ClientOptionItem_Boolean.Create("EnableMapBackGround", Main.EnableMapBackGround, __instance);
        }
        //if (PrunkMode == null || PrunkMode.ToggleButton == null)
        //{
        //    PrunkMode = ClientOptionItem_Boolean.Create("PrunkMode", Main.PrunkMode, __instance);
        //}
        if (DisableVanillaSound == null || DisableVanillaSound.ToggleButton == null)
        {
            DisableVanillaSound = ClientOptionItem_Boolean.Create("DisableVanillaSound", Main.DisableVanillaSound, __instance, () => {
                if (Main.DisableVanillaSound.Value)
                    CustomSoundsManager.StopPlay();
            });
        }
        if (DisableFAC == null || DisableFAC.ToggleButton == null)
        {
            DisableFAC = ClientOptionItem_Boolean.Create("DisableFAC", Main.DisableFAC, __instance);
        }
        if (UnloadMod == null || UnloadMod.ToggleButton == null)
        {
            UnloadMod = ClientActionItem.Create("UnloadMod", ModUnloaderScreen.Show, __instance);
        }
        if (DumpLog == null || DumpLog.ToggleButton == null)
        {
            DumpLog = ClientActionItem.Create("DumpLog", () => Utils.DumpLog(), __instance);
        }

        if ((VersionCheat == null || VersionCheat.ToggleButton == null) && DebugModeManager.AmDebugger)
        {
            VersionCheat = ClientOptionItem_Boolean.Create("VersionCheat", Main.VersionCheat, __instance);
        }

        if ((GodMode == null || GodMode.ToggleButton == null) && DebugModeManager.AmDebugger)
        {
            GodMode = ClientOptionItem_Boolean.Create("GodMode", Main.GodMode, __instance);
        }
        
        if ((NoGameEnd == null || NoGameEnd.ToggleButton == null) && DebugModeManager.AmDebugger)
        {
            NoGameEnd = ClientOptionItem_Boolean.Create("NoGameEnd", Main.NoGameEnd, __instance);
        }
        
        var mouseMoveToggle = __instance.DisableMouseMovement;
        if ((SoundBtn == null || SoundBtn.ToggleButton == null))
        {
            SoundBtn = ClientFeatureItem.Create("SoundOption", () =>
            {
                MyMusicPanel.CustomBackground?.gameObject?.SetActive(true);
            }, __instance);
        }

        if ((AudioManagementBtn == null || AudioManagementBtn.ToggleButton == null))
        {
            AudioManagementBtn = ClientFeatureItem.Create("SoundManager", () =>
            {
                AudioManagementPanel.CustomBackground?.gameObject?.SetActive(true);
            }, __instance);
        }

        SoundBtn.ToggleButton.Text.text = Translator.GetString("SoundOptions");
        SoundBtn.ToggleButton.GetComponent<PassiveButton>().enabled = true;
        SoundBtn.ToggleButton.Background.color = ColorHelper.ModColor32;
        AudioManagementBtn.ToggleButton.Text.text = Translator.GetString("AudioManagementOptions");
        AudioManagementBtn.ToggleButton.GetComponent<PassiveButton>().enabled = true;
        AudioManagementBtn.ToggleButton.Background.color = ColorHelper.ModColor32;
        if (!XtremeGameData.GameStates.IsNotJoined)
        {
            AudioManagementBtn.ToggleButton.Text.text = Translator.GetString("AudioManagementOptions") + "|" + Translator.GetString("OnlyAvailableInMainMenu");
            AudioManagementBtn.ToggleButton.GetComponent<PassiveButton>().enabled = false;
            AudioManagementBtn.ToggleButton.Background.color = Palette.DisabledGrey;
        }


        MyMusicPanel.Init(__instance);
        AudioManagementPanel.Init(__instance);

        if (ModUnloaderScreen.Popup == null)
            ModUnloaderScreen.Init(__instance);

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
        AudioManagementPanel.Hide();
    }
}
