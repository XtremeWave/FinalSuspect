using AmongUs.GameOptions;
using HarmonyLib;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using static FinalSuspect_Xtreme.Patches.NormalLobbyViewSettingsPanePatch;
using UnityEngine.ProBuilder;

namespace FinalSuspect_Xtreme.Patches;
[HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.UpdateValuesAndText))]
class RoleOptionSettingPatch
{
    public static void Postfix(RoleOptionSetting __instance)
    {
        var rolecolor = Utils.GetRoleColor(__instance.Role.Role);
        __instance.labelSprite.color = Utils.ShadeColor(rolecolor, 0.2f);
        __instance.titleText.color = Color.white;
    }
}
[HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.Update))]
class RolesSettingsMenuPatch
{
    static GameObject AllButton = null;
    static GameObject EngineerButton = null;
    static GameObject GuardianAngelButton = null;
    static GameObject ScientistButton = null;
    static GameObject TrackerButton = null;
    static GameObject NoiseMakerButton = null;
    static GameObject ShapeShifterButton = null;
    static GameObject PhantomButton = null;
    public static void Postfix()
    {
        if (!XtremeGameData.GameStates.IsNormalGame) return;
        try
        { 
        #region 顶部按钮
        var header = GameObject.Find("HeaderButtons");

        if (AllButton == null)
        {
            AllButton = header.transform.FindChild("AllButton").gameObject;
        }
        if (EngineerButton == null)
        {
            EngineerButton = header.transform.GetChild(4).gameObject;
        }
        if (GuardianAngelButton == null)
        {
            GuardianAngelButton = header.transform.GetChild(5).gameObject;
        }
        if (ScientistButton == null)
        {
            ScientistButton = header.transform.GetChild(6).gameObject;
        }
        if (TrackerButton == null)
        {
            TrackerButton = header.transform.GetChild(7).gameObject;
        }
        if (NoiseMakerButton == null)
        {
            NoiseMakerButton = header.transform.GetChild(8).gameObject;
        }
        if (ShapeShifterButton == null)
        {
            ShapeShifterButton = header.transform.GetChild(9).gameObject;
        }
        if (PhantomButton == null)
        {
            PhantomButton = header.transform.GetChild(10).gameObject;
        }

        AllButton.transform.FindChild("Highlight").gameObject.GetComponent<SpriteRenderer>().color = ColorHelper.ModColor32;
        AllButton.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().color = ColorHelper.ModColor32;
        AllButton.transform.FindChild("Selected").gameObject.GetComponent<SpriteRenderer>().color = ColorHelper.ModColor32;
        var color = AllButton.transform.FindChild("Text").gameObject.GetComponent<TextMeshPro>().color;
        if (color == Color.white || color == ColorHelper.ModColor32)
            AllButton.transform.FindChild("Text").gameObject.GetComponent<TextMeshPro>().color = ColorHelper.ModColor32;
        else
            AllButton.transform.FindChild("Text").gameObject.GetComponent<TextMeshPro>().color = new Color(0.45f, 0.45f, 0.65f);
        SetColor(EngineerButton, Utils.GetRoleColor(RoleTypes.Engineer), Utils.GetRoleColor(RoleTypes.Crewmate));
        SetColor(GuardianAngelButton, Utils.GetRoleColor(RoleTypes.GuardianAngel), Utils.GetRoleColor(RoleTypes.Crewmate));
        SetColor(ScientistButton, Utils.GetRoleColor(RoleTypes.Scientist), Utils.GetRoleColor(RoleTypes.Crewmate));
        SetColor(TrackerButton, Utils.GetRoleColor(RoleTypes.Tracker), Utils.GetRoleColor(RoleTypes.Crewmate));
        SetColor(NoiseMakerButton, Utils.GetRoleColor(RoleTypes.Noisemaker), Utils.GetRoleColor(RoleTypes.Crewmate));
        SetColor(ShapeShifterButton, Utils.GetRoleColor(RoleTypes.Shapeshifter), Utils.GetRoleColor(RoleTypes.Impostor));
        SetColor(PhantomButton, Utils.GetRoleColor(RoleTypes.Phantom), Utils.GetRoleColor(RoleTypes.Impostor));
        #endregion
        var RoleArea = GameObject.Find("ROLES TAB").transform.FindChild("Scroller").FindChild("SliderInner");
        SetColorForCat(RoleArea.FindChild("ChancesTab").FindChild("CategoryHeaderMasked").gameObject, Color.green);
        SetColorForCat(RoleArea.FindChild("AdvancedTab").FindChild("CategoryHeaderMasked").gameObject, Color.blue);
        }
        catch { }
    }
    static void SetColor(GameObject obj, Color iconcolor, Color bgcolor)
    {
        obj.transform.FindChild("SelectedHighlight").gameObject.GetComponent<SpriteRenderer>().color = bgcolor;
        obj.transform.FindChild("HoverHighlight").gameObject.GetComponent<SpriteRenderer>().color = bgcolor; 
        obj.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().color = bgcolor;
        obj.transform.FindChild("RoleIcon").gameObject.GetComponent<SpriteRenderer>().color = iconcolor;
    }
}
[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
class NormalGameOptionsMenuPatch
{
    static GameObject ImpostorCat = null;
    static GameObject CrewmateCat = null;
    static GameObject MeetingCat = null;
    static GameObject TaskCat = null;

    static GameObject ImpostorSettingBanner1 = null;
    static GameObject ImpostorSettingBanner2 = null;
    static GameObject ImpostorSettingBanner3 = null;
    static GameObject ImpostorSettingBanner4 = null;
    static GameObject CrewmateSettingBanner1 = null;
    static GameObject CrewmateSettingBanner2 = null;
    static GameObject MeetingSettingBanner1 = null;
    static GameObject MeetingSettingBanner2 = null;
    static GameObject MeetingSettingBanner3 = null;
    static GameObject MeetingSettingBanner4 = null;
    static GameObject MeetingSettingBanner5 = null;
    static GameObject MeetingSettingBanner6 = null;
    static GameObject TaskSettingBanner1 = null;
    static GameObject TaskSettingBanner2 = null;
    static GameObject TaskSettingBanner3 = null;
    static GameObject TaskSettingBanner4 = null;
    static GameObject TaskSettingBanner5 = null;
    public static void Postfix()
    {
        if (!XtremeGameData.GameStates.IsNormalGame) return;
        try
        { 
        var SetArea = GameObject.Find("GAME SETTINGS TAB").transform.FindChild("Scroller").FindChild("SliderInner");
        Transform[] Banners = SetArea.GetComponentsInChildren<Transform>(true);

        List<GameObject> Cats = new();

        foreach (var Banner in Banners)
        {
            if (Banner.name == "CategoryHeaderMasked(Clone)")
            {
                Cats.Add(Banner.gameObject);
            }
        }

        if (ImpostorCat == null)
        {
            ImpostorCat = Cats[0];
        }
        if (CrewmateCat == null)
        {
            CrewmateCat = Cats[1];
        }
        if (MeetingCat == null)
        {
            MeetingCat = Cats[2];
        }
        if (TaskCat == null)
        {
            TaskCat = Cats[3];
        }


        List<GameObject> GameOptions = new();

        foreach (var Banner in Banners)
        {
            if (Banner.name.Contains("GameOption"))
            {
                GameOptions.Add(Banner.gameObject);
            }
        }

        if (ImpostorSettingBanner1 == null)
        {
            ImpostorSettingBanner1 = GameOptions[0];
        }
        if (ImpostorSettingBanner2 == null)
        {
            ImpostorSettingBanner2 = GameOptions[1];
        }
        if (ImpostorSettingBanner3 == null)
        {
            ImpostorSettingBanner3 = GameOptions[2];
        }
        if (ImpostorSettingBanner4 == null)
        {
            ImpostorSettingBanner4 = GameOptions[3];
        }
        if (CrewmateSettingBanner1 == null)
        {
            CrewmateSettingBanner1 = GameOptions[4];
        }
        if (CrewmateSettingBanner2 == null)
        {
            CrewmateSettingBanner2 = GameOptions[5];
        }
        if (MeetingSettingBanner1 == null)
        {
            MeetingSettingBanner1 = GameOptions[6];
        }
        if (MeetingSettingBanner2 == null)
        {
            MeetingSettingBanner2 = GameOptions[7];
        }
        if (MeetingSettingBanner3 == null)
        {
            MeetingSettingBanner3 = GameOptions[8];
        }
        if (MeetingSettingBanner4 == null)
        {
            MeetingSettingBanner4 = GameOptions[9];
        }
        if (MeetingSettingBanner5 == null)
        {
            MeetingSettingBanner5 = GameOptions[10];
        }
        if (MeetingSettingBanner6 == null)
        {
            MeetingSettingBanner6 = GameOptions[11];
        }
        if (TaskSettingBanner1 == null)
        {
            TaskSettingBanner1 = GameOptions[12];
        }
        if (TaskSettingBanner2 == null)
        {
            TaskSettingBanner2 = GameOptions[13];
        }
        if (TaskSettingBanner3 == null)
        {
            TaskSettingBanner3 = GameOptions[14];
        }
        if (TaskSettingBanner4 == null)
        {
            TaskSettingBanner4 = GameOptions[15];
        }
        if (TaskSettingBanner5 == null)
        {
            TaskSettingBanner5 = GameOptions[16];
        }
        SetColorForCat(ImpostorCat, Utils.GetRoleColor(RoleTypes.Impostor));
        SetColorForCat(CrewmateCat, Utils.GetRoleColor(RoleTypes.Crewmate));
        SetColorForCat(MeetingCat, Color.yellow);
        SetColorForCat(TaskCat, Color.green);

        SetColorForSettingsOpt_StringAndNumber(ImpostorSettingBanner1, Utils.GetRoleColor(RoleTypes.Impostor));
        SetColorForSettingsOpt_StringAndNumber(ImpostorSettingBanner2, Utils.GetRoleColor(RoleTypes.Impostor));
        SetColorForSettingsOpt_StringAndNumber(ImpostorSettingBanner3, Utils.GetRoleColor(RoleTypes.Impostor));
        SetColorForSettingsOpt_StringAndNumber(ImpostorSettingBanner4, Utils.GetRoleColor(RoleTypes.Impostor));

        SetColorForSettingsOpt_StringAndNumber(CrewmateSettingBanner1, Utils.GetRoleColor(RoleTypes.Crewmate));
        SetColorForSettingsOpt_StringAndNumber(CrewmateSettingBanner2, Utils.GetRoleColor(RoleTypes.Crewmate));

        SetColorForSettingsOpt_StringAndNumber(MeetingSettingBanner1, Color.yellow);
        SetColorForSettingsOpt_StringAndNumber(MeetingSettingBanner2, Color.yellow);
        SetColorForSettingsOpt_StringAndNumber(MeetingSettingBanner3, Color.yellow);
        SetColorForSettingsOpt_StringAndNumber(MeetingSettingBanner4, Color.yellow);
        SetColorForSettingsOpt_Checkbox(MeetingSettingBanner5, Color.yellow);
        SetColorForSettingsOpt_Checkbox(MeetingSettingBanner6, Color.yellow);

        SetColorForSettingsOpt_StringAndNumber(TaskSettingBanner1, Color.green);
        SetColorForSettingsOpt_StringAndNumber(TaskSettingBanner2, Color.green);
        SetColorForSettingsOpt_StringAndNumber(TaskSettingBanner3, Color.green);
        SetColorForSettingsOpt_StringAndNumber(TaskSettingBanner4, Color.green);
        SetColorForSettingsOpt_Checkbox(TaskSettingBanner5, Color.green);

        }
        catch { }


    }
}
[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
class HnSGameOptionsMenuPatch
{
    static GameObject CrewmateCat = null;
    static GameObject ImpostorCat = null;
    static GameObject LastHiddenCat = null;
    static GameObject TaskCat = null;

    static GameObject CrewmateSettingBanner1 = null;
    static GameObject CrewmateSettingBanner2 = null;
    static GameObject CrewmateSettingBanner3 = null;
    static GameObject CrewmateSettingBanner4 = null;
    static GameObject CrewmateSettingBanner5 = null;
    static GameObject CrewmateSettingBanner6 = null;
    static GameObject CrewmateSettingBanner7 = null;
    static GameObject CrewmateSettingBanner8 = null;
    static GameObject ImpostorSettingBanner1 = null;
    static GameObject ImpostorSettingBanner2 = null;
    static GameObject ImpostorSettingBanner3 = null;
    static GameObject LastHiddenSettingBanner1 = null;
    static GameObject LastHiddenSettingBanner2 = null;
    static GameObject LastHiddenSettingBanner3 = null;
    static GameObject LastHiddenSettingBanner4 = null;
    static GameObject LastHiddenSettingBanner5 = null;
    static GameObject TaskSettingBanner1 = null;
    static GameObject TaskSettingBanner2 = null;
    static GameObject TaskSettingBanner3 = null;
    public static void Postfix()
    {
        if (XtremeGameData.GameStates.IsNormalGame) return;
        try
        {
            var SetArea = GameObject.Find("GAME SETTINGS TAB").transform.FindChild("Scroller").FindChild("SliderInner");
            Transform[] Banners = SetArea.GetComponentsInChildren<Transform>(true);

            List<GameObject> Cats = new();

            foreach (var Banner in Banners)
            {
                if (Banner.name == "CategoryHeaderMasked(Clone)")
                {
                    Cats.Add(Banner.gameObject);
                }
            }
            if (CrewmateCat == null)
            {
                CrewmateCat = Cats[0];
            }
            if (ImpostorCat == null)
            {
                ImpostorCat = Cats[1];
            }

            if (LastHiddenCat == null)
            {
                LastHiddenCat = Cats[2];
            }
            if (TaskCat == null)
            {
                TaskCat = Cats[3];
            }


            List<GameObject> GameOptions = new();

            foreach (var Banner in Banners)
            {
                if (Banner.name.Contains("GameOption"))
                {
                    GameOptions.Add(Banner.gameObject);
                }
            }

            if (CrewmateSettingBanner1 == null)
            {
                CrewmateSettingBanner1 = GameOptions[0];
            }
            if (CrewmateSettingBanner2 == null)
            {
                CrewmateSettingBanner2 = GameOptions[1];
            }
            if (CrewmateSettingBanner3 == null)
            {
                CrewmateSettingBanner3 = GameOptions[2];
            }
            if (CrewmateSettingBanner4 == null)
            {
                CrewmateSettingBanner4 = GameOptions[3];
            }
            if (CrewmateSettingBanner5 == null)
            {
                CrewmateSettingBanner5 = GameOptions[4];
            }
            if (CrewmateSettingBanner6 == null)
            {
                CrewmateSettingBanner6 = GameOptions[5];
            }
            if (CrewmateSettingBanner7 == null)
            {
                CrewmateSettingBanner7 = GameOptions[6];
            }
            if (CrewmateSettingBanner8 == null)
            {
                CrewmateSettingBanner8 = GameOptions[7];
            }

            if (ImpostorSettingBanner1 == null)
            {
                ImpostorSettingBanner1 = GameOptions[8];
            }
            if (ImpostorSettingBanner2 == null)
            {
                ImpostorSettingBanner2 = GameOptions[9];
            }
            if (ImpostorSettingBanner3 == null)
            {
                ImpostorSettingBanner3 = GameOptions[10];
            }
            if (LastHiddenSettingBanner1 == null)
            {
                LastHiddenSettingBanner1 = GameOptions[11];
            }
            if (LastHiddenSettingBanner2 == null)
            {
                LastHiddenSettingBanner2 = GameOptions[12];
            }
            if (LastHiddenSettingBanner3 == null)
            {
                LastHiddenSettingBanner3 = GameOptions[13];
            }
            if (LastHiddenSettingBanner4 == null)
            {
                LastHiddenSettingBanner4 = GameOptions[14];
            }
            if (LastHiddenSettingBanner5 == null)
            {
                LastHiddenSettingBanner5 = GameOptions[15];
            }
            if (TaskSettingBanner1 == null)
            {
                TaskSettingBanner1 = GameOptions[16];
            }
            if (TaskSettingBanner2 == null)
            {
                TaskSettingBanner2 = GameOptions[17];
            }
            if (TaskSettingBanner3 == null)
            {
                TaskSettingBanner3 = GameOptions[18];
            }
            SetColorForCat(CrewmateCat, Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForCat(ImpostorCat, Utils.GetRoleColor(RoleTypes.Impostor));
            SetColorForCat(LastHiddenCat, Palette.Purple);
            SetColorForCat(TaskCat, Color.green);

            SetColorForSettingsOpt_StringAndNumber(CrewmateSettingBanner1, Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForSettingsOpt_StringAndNumber(CrewmateSettingBanner2, Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForSettingsOpt_StringAndNumber(CrewmateSettingBanner3, Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForSettingsOpt_StringAndNumber(CrewmateSettingBanner4, Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForSettingsOpt_Checkbox(CrewmateSettingBanner5, Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForSettingsOpt_StringAndNumber(CrewmateSettingBanner6, Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForSettingsOpt_StringAndNumber(CrewmateSettingBanner7, Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForSettingsOpt_Checkbox(CrewmateSettingBanner8, Utils.GetRoleColor(RoleTypes.Crewmate));

            SetColorForSettingsOpt_StringAndNumber(ImpostorSettingBanner1, Utils.GetRoleColor(RoleTypes.Impostor));
            SetColorForSettingsOpt_StringAndNumber(ImpostorSettingBanner2, Utils.GetRoleColor(RoleTypes.Impostor));
            SetColorForSettingsOpt_StringAndNumber(ImpostorSettingBanner3, Utils.GetRoleColor(RoleTypes.Impostor));

            SetColorForSettingsOpt_StringAndNumber(LastHiddenSettingBanner1, Palette.Purple);
            SetColorForSettingsOpt_Checkbox(LastHiddenSettingBanner2, Palette.Purple);
            SetColorForSettingsOpt_StringAndNumber(LastHiddenSettingBanner3, Palette.Purple);
            SetColorForSettingsOpt_StringAndNumber(LastHiddenSettingBanner4, Palette.Purple);
            SetColorForSettingsOpt_Checkbox(LastHiddenSettingBanner5, Palette.Purple);

            SetColorForSettingsOpt_StringAndNumber(TaskSettingBanner1, Color.green);
            SetColorForSettingsOpt_StringAndNumber(TaskSettingBanner2, Color.green);
            SetColorForSettingsOpt_StringAndNumber(TaskSettingBanner3, Color.green);


        }
        catch { }

    }
}

[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Update))]
class GameSettingMenuPatch
{
    
    static GameObject GamePresetButton = null;
    static GameObject GameSettingsButton = null;
    static GameObject RoleSettingsButton = null;
    public static void Postfix()
    {
        try
        {
            var Panel = GameObject.Find("LeftPanel");

            if (GamePresetButton == null)
            {
                GamePresetButton = Panel.transform.FindChild("GamePresetButton").gameObject;
            }
            if (GameSettingsButton == null)
            {
                GameSettingsButton = Panel.transform.FindChild("GameSettingsButton").gameObject;
            }
            if (RoleSettingsButton == null && XtremeGameData.GameStates.IsNormalGame)
            {
                RoleSettingsButton = Panel.transform.FindChild("RoleSettingsButton").gameObject;
            }

            SetColor(GamePresetButton, new Color32(205, 255, 253, 255));
            SetColor(GameSettingsButton, new Color32(206, 205, 253, 255));
            SetColor(RoleSettingsButton, new Color32(185, 255, 181, 255));

            var ps = GameObject.Find("PanelSprite");
            ps.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.4f);
            ps.transform.FindChild("LeftSideTint").gameObject.GetComponent<SpriteRenderer>().color = new Color(0.1176f, 0.1176f, 0.1176f, 0.8f);

        }
        catch { }
    }
    static void SetColor(GameObject obj, Color bgcolor)
    {
        if (obj == null) return;
        obj.transform.FindChild("Highlight").gameObject.GetComponent<SpriteRenderer>().color = bgcolor;
        obj.transform.FindChild("Selected").gameObject.GetComponent<SpriteRenderer>().color = bgcolor;
        obj.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().color = bgcolor;
    }
}