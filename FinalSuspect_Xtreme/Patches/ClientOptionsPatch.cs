using HarmonyLib;
using FinalSuspect_Xtreme.Modules.ClientOptions;
using FinalSuspect_Xtreme.Modules.SoundInterface;
using UnityEngine;
using FinalSuspect_Xtreme.Modules.Managers;

namespace FinalSuspect_Xtreme;

[HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
public static class OptionsMenuBehaviourStartPatch
{
    private static ClientOptionItem_Boolean UnlockFPS;
    private static ClientOptionItem_String AprilFoolsMode;
    private static ClientOptionItem_Boolean KickPlayerFriendCodeNotExist;
    private static ClientOptionItem_Boolean ApplyDenyNameList;
    private static ClientOptionItem_Boolean ApplyBanList;
    private static ClientOptionItem_Boolean AutoStartGame;
    private static ClientOptionItem_Boolean AutoEndGame;
    private static ClientOptionItem_Boolean EnableMapBackGround;
    private static ClientOptionItem_Boolean EnableRoleBackGround;
    private static ClientOptionItem_Boolean DisableVanillaSound;
    private static ClientActionItem UnloadMod;
    private static ClientActionItem DumpLog;
    private static ClientOptionItem_Boolean VersionCheat;
    private static ClientOptionItem_Boolean GodMode;

    public static ClientFeatureItem SoundBtn;
    public static ClientFeatureItem SoundManagerBtn;

    private static bool reseted = false;
    public static void Postfix(OptionsMenuBehaviour __instance)
    {
        if (__instance.DisableMouseMovement == null) return;

        

        if (!reseted || !DebugModeManager.AmDebugger)
        {
            reseted = true;
            Main.VersionCheat.Value = false;
            Main.GodMode.Value = false;
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
        if (AprilFoolsMode == null || AprilFoolsMode.ToggleButton == null)
        {
            AprilFoolsMode = ClientOptionItem_String.Create(
                Main.AprilFoolsMode.Value ?? Main.allAprilFoolsModes[0] 
                 
                , Main.AprilFoolsMode, __instance, Main.allAprilFoolsModes, SwitchHorseMode);
            static void SwitchHorseMode()
            {
                AprilFoolsMode.UpdateToggle(Main.allAprilFoolsModes);
                if (Main.AprilFoolsMode.Value == Main.allAprilFoolsModes[1])
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    pc.MyPhysics.SetBodyType(pc.BodyType);
                    if (pc.BodyType == PlayerBodyTypes.Normal)
                    {
                        pc.cosmetics.currentBodySprite.BodySprite.transform.localScale = new(0.5f, 0.5f, 1f);
                    }
                }
                
            }
            if (!XtremeGameData.GameStates.IsNotJoined)
            {
                AprilFoolsMode.ToggleButton.GetComponent<PassiveButton>().enabled = false;
                AprilFoolsMode.ToggleButton.Background.color = Palette.DisabledGrey;
                AprilFoolsMode.ToggleButton.Text.text = Translator.GetString("ChangeOutfit") + "|" + Translator.GetString("OnlyAvailableInMainMenu");
            }
            else
            {
                AprilFoolsMode.UpdateToggle(Main.allAprilFoolsModes);
                AprilFoolsMode.UpdateName(Main.AprilFoolsMode.Value);
                AprilFoolsMode.ToggleButton.GetComponent<PassiveButton>().enabled =true;
            }
        }
        
        if (KickPlayerFriendCodeNotExist== null || KickPlayerFriendCodeNotExist.ToggleButton == null)
        {
            KickPlayerFriendCodeNotExist = ClientOptionItem_Boolean.Create("KickPlayerFriendCodeNotExist", Main.KickPlayerFriendCodeNotExist, __instance);
        }
        if (ApplyBanList == null || ApplyBanList.ToggleButton == null)
        {
            ApplyBanList = ClientOptionItem_Boolean.Create("ApplyBanList", Main.ApplyBanList, __instance);
        }
        if (ApplyDenyNameList == null || ApplyDenyNameList.ToggleButton == null)
        {
            ApplyDenyNameList = ClientOptionItem_Boolean.Create("ApplyDenyNameList", Main.ApplyDenyNameList, __instance);
        }

        if (DisableVanillaSound == null || DisableVanillaSound.ToggleButton == null)
        {
            DisableVanillaSound = ClientOptionItem_Boolean.Create("DisableVanillaSound", Main.DisableVanillaSound, __instance, () => {
                if (Main.DisableVanillaSound.Value)
                    CustomSoundsManager.StopPlay();
            });
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
        
        var mouseMoveToggle = __instance.DisableMouseMovement;
        if ((SoundBtn == null || SoundBtn.ToggleButton == null))
        {
            SoundBtn = ClientFeatureItem.Create("SoundOption", () =>
            {
                SoundPanel.CustomBackground?.gameObject?.SetActive(true);
            }, __instance);
        }

        if ((SoundManagerBtn == null || SoundManagerBtn.ToggleButton == null))
        {
            SoundManagerBtn = ClientFeatureItem.Create("SoundManager", () =>
            {
                SoundManagerPanel.CustomBackground?.gameObject?.SetActive(true);
            }, __instance);
        }

        SoundBtn.ToggleButton.Text.text = Translator.GetString("SoundOptions");
        SoundBtn.ToggleButton.GetComponent<PassiveButton>().enabled = true;
        SoundBtn.ToggleButton.Background.color = Main.ModColor32;
        SoundManagerBtn.ToggleButton.Text.text = Translator.GetString("SoundManagerOptions");
        SoundManagerBtn.ToggleButton.GetComponent<PassiveButton>().enabled = true;
        SoundManagerBtn.ToggleButton.Background.color = Main.ModColor32;
        if (!XtremeGameData.GameStates.IsNotJoined)
        {
            SoundManagerBtn.ToggleButton.Text.text = Translator.GetString("SoundManagerOptions") + "|" + Translator.GetString("OnlyAvailableInMainMenu");
            SoundManagerBtn.ToggleButton.GetComponent<PassiveButton>().enabled = false;
            SoundManagerBtn.ToggleButton.Background.color = Palette.DisabledGrey;
        }


        SoundPanel.Init(__instance);
        SoundManagerPanel.Init(__instance);

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
        SoundPanel.Hide();
        SoundManagerPanel.Hide();
    }
}
