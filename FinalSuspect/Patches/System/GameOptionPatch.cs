using System.Collections.Generic;
using AmongUs.GameOptions;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game;
using TMPro;
using UnityEngine;

namespace FinalSuspect.Patches.System;
[HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.UpdateValuesAndText))]
class RoleOptionSettingPatch
{
    public static void Postfix(RoleOptionSetting __instance)
    {
        var rolecolor = Utils.GetRoleColor(__instance.Role.Role);
        __instance.labelSprite.color = rolecolor.ShadeColor(0.2f);
        __instance.titleText.color = Color.white;
    }
}
[HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.Update))]
class RolesSettingsMenuPatch
{
    private static readonly List<Color32> rolecolors =
    [
        Utils.GetRoleColor(RoleTypes.Engineer),
        Utils.GetRoleColor(RoleTypes.GuardianAngel),
        Utils.GetRoleColor(RoleTypes.Scientist),
        Utils.GetRoleColor(RoleTypes.Tracker),
        Utils.GetRoleColor(RoleTypes.Noisemaker),
        Utils.GetRoleColor(RoleTypes.Shapeshifter),
        Utils.GetRoleColor(RoleTypes.Phantom)
    ];

    public static void Postfix()
    {
        if (!XtremeGameData.GameStates.IsNormalGame) return;

        try
        {
            ConfigureHeaderButtons();
            SetRoleAreaColors();
        }
        catch
        { }
    }

    private static void ConfigureHeaderButtons()
    {
        var header = GameObject.Find("HeaderButtons");
        var headerbuttons = new List<GameObject>();

        for (var i = 4; i <= 10; i++)
        {
            headerbuttons.Add(header.transform.GetChild(i).gameObject);
        }

        var index = 0;
        foreach (var button in headerbuttons)
        {
            var roleColor = index <= 4 ? Utils.GetRoleColor(RoleTypes.Crewmate) : Utils.GetRoleColor(RoleTypes.Impostor);
            SetColor(button, rolecolors[index], roleColor);
            index++;
        }

        ConfigureAllButtonColors();
    }

    private static void ConfigureAllButtonColors()
    {
        var AllButton = GameObject.Find("HeaderButtons").transform.FindChild("AllButton").gameObject;
        AllButton.transform.FindChild("Highlight").gameObject.GetComponent<SpriteRenderer>().color =
            AllButton.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().color =
                AllButton.transform.FindChild("Selected").gameObject.GetComponent<SpriteRenderer>().color =
                    ColorHelper.ModColor32;

        var text = AllButton.transform.FindChild("Text").gameObject.GetComponent<TextMeshPro>();
        if (text.color == Color.white || text.color == ColorHelper.ModColor32)
        {
            text.color = ColorHelper.ModColor32;
        }
        else
        {
            text.color = new Color(0.45f, 0.45f, 0.65f);
        }
    }

    private static void SetRoleAreaColors()
    {
        var RoleArea = GameObject.Find("ROLES TAB").transform.FindChild("Scroller").FindChild("SliderInner");
        GameOptionsMenuPatch.SetColorForCat(RoleArea.FindChild("ChancesTab").FindChild("CategoryHeaderMasked").gameObject, Color.green);
        GameOptionsMenuPatch.SetColorForCat(RoleArea.FindChild("AdvancedTab").FindChild("CategoryHeaderMasked").gameObject, Color.blue);
    }

    private static void SetColor(GameObject obj, Color iconcolor, Color bgcolor)
    {
        obj.transform.FindChild("SelectedHighlight").gameObject.GetComponent<SpriteRenderer>().color = bgcolor;
        obj.transform.FindChild("HoverHighlight").gameObject.GetComponent<SpriteRenderer>().color = bgcolor;
        obj.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().color = bgcolor;
        obj.transform.FindChild("RoleIcon").gameObject.GetComponent<SpriteRenderer>().color = iconcolor;
    }
    
}
[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
internal class GameOptionsMenuPatch
{
    private static readonly List<Color32> Normalbannercolors =
    [
        Utils.GetRoleColor(RoleTypes.Impostor),
        Utils.GetRoleColor(RoleTypes.Crewmate),
        Color.yellow,
        Color.green
    ];
    private static readonly List<Color32> HnSbannercolors =
    [
        Utils.GetRoleColor(RoleTypes.Crewmate),
        Utils.GetRoleColor(RoleTypes.Impostor),
        Palette.Purple,
        Color.green
    ];
    public static void Postfix()
    {


        var setArea = GameObject.Find("GAME SETTINGS TAB").transform.FindChild("Scroller").FindChild("SliderInner");
        Transform[] banners = setArea.GetComponentsInChildren<Transform>(true);
        if (XtremeGameData.GameStates.IsNormalGame)
        {
            var headerindex = 0;
            var numindex = 0;
            var boxindex = 0;
            foreach (var banner in banners)
            {

                if (banner.name == "CategoryHeaderMasked(Clone)")
                {
                    SetColorForCat(banner.gameObject, Normalbannercolors[headerindex]);
                    headerindex++;
                }
                else if (banner.name.Contains("Num") || banner.name.Contains("Str"))
                {
                    Color color;
                    if (numindex <= 3)
                        color = Normalbannercolors[0];
                    else if (numindex <= 5)
                        color = Normalbannercolors[1];
                    else if (numindex <= 9)
                        color = Normalbannercolors[2];
                    else
                        color = Normalbannercolors[3];
                    SetColorForSettingsOpt_StringAndNumber(banner.gameObject, color);
                    numindex++;
                }
                else if (banner.name.Contains("Checkbox"))
                {
                    Color color;
                    if (boxindex <= 1)
                        color = Normalbannercolors[2];
                    else
                        color = Normalbannercolors[3];
                    SetColorForSettingsOpt_Checkbox(banner.gameObject, color);
                    boxindex++;

                }
            }
        }
        else
        {
            var headerindex = 0;
            var numindex = 0;
            var boxindex = 0;
            foreach (var banner in banners)
            {
                if (banner.name == "CategoryHeaderMasked(Clone)")
                {
                    SetColorForCat(banner.gameObject, HnSbannercolors[headerindex]);
                    headerindex++;
                }
                else if (banner.name.Contains("Num") || banner.name.Contains("Str")|| banner.name.Contains("Play"))
                {
                    Color color;
                    if (numindex <= 5)
                        color = HnSbannercolors[0];
                    else if (numindex <= 8)
                        color = HnSbannercolors[1];
                    else if (numindex <= 11)
                        color = HnSbannercolors[2];
                    else 
                        color = HnSbannercolors[3];
                    SetColorForSettingsOpt_StringAndNumber(banner.gameObject, color); 
                    numindex++;
                }
                else if (banner.name.Contains("Checkbox"))
                {
                    Color color;
                    if (boxindex <= 1)
                        color = HnSbannercolors[0];
                    else 
                        color = HnSbannercolors[2];
                    SetColorForSettingsOpt_Checkbox(banner.gameObject, color); 
                    boxindex++;
                
                }
            }

        }
    }
    internal static void SetColorForCat(GameObject obj, Color color)
    {
        if (obj == null) return;
        obj.transform.FindChild("LabelSprite").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.18f);
        obj.transform.FindChild("DividerImage").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.18f);
    }
    static void SetColorForSettingsOpt_StringAndNumber(GameObject obj, Color color)
    {
        obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.38f);
        obj.transform.FindChild("ValueBox").gameObject.GetComponent<SpriteRenderer>().color = color;
    }
    static void SetColorForSettingsOpt_Checkbox(GameObject obj, Color color)
    {
        obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.38f);
        obj.transform.FindChild("Toggle").FindChild("InactiveSprite").gameObject.GetComponent<SpriteRenderer>().color = color;
    }
}

[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Update))]
class GameSettingMenuPatch
{
    
    static GameObject GamePresetButton;
    static GameObject GameSettingsButton;
    static GameObject RoleSettingsButton;
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