using AmongUs.GameOptions;
using FinalSuspect.Helpers;
using TMPro;
using UnityEngine;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.UpdateValuesAndText))]
internal class RoleOptionSettingPatch
{
    public static void Postfix(RoleOptionSetting __instance)
    {
        var roleColor = GetRoleColor(__instance.Role.Role);
        __instance.labelSprite.color = roleColor.ShadeColor(0.2f);
        __instance.titleText.color = Color.white;
    }
}

[HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.Update))]
internal class RolesSettingsMenuPatch
{
    private static readonly List<Color32> rolecolors =
    [
        GetRoleColor(RoleTypes.Engineer),
        GetRoleColor(RoleTypes.GuardianAngel),
        GetRoleColor(RoleTypes.Scientist),
        GetRoleColor(RoleTypes.Tracker),
        GetRoleColor(RoleTypes.Noisemaker),
        GetRoleColor(RoleTypes.Shapeshifter),
        GetRoleColor(RoleTypes.Phantom)
    ];

    public static void Postfix()
    {
        if (!IsNormalGame) return;
        try
        {
            ConfigureHeaderButtons();
            SetRoleAreaColors();
        }
        catch
        {
            /* ignored */
        }
    }

    private static void ConfigureHeaderButtons()
    {
        var header = GameObject.Find("HeaderButtons");
        var headerbuttons = new List<GameObject>();

        for (var i = 4; i <= 10; i++) headerbuttons.Add(header.transform.GetChild(i).gameObject);

        var index = 0;
        foreach (var button in headerbuttons)
        {
            var roleColor = index <= 4 ? GetRoleColor(RoleTypes.Crewmate) : GetRoleColor(RoleTypes.Impostor);
            SetColor(button, rolecolors[index], roleColor);
            index++;
        }

        ConfigureAllButtonColors();
    }

    private static void ConfigureAllButtonColors()
    {
        var allButton = GameObject.Find("HeaderButtons").transform.FindChild("AllButton").gameObject;
        allButton.transform.FindChild("Highlight").gameObject.GetComponent<SpriteRenderer>().color =
            allButton.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().color =
                allButton.transform.FindChild("Selected").gameObject.GetComponent<SpriteRenderer>().color =
                    ColorHelper.FSColor;

        var text = allButton.transform.FindChild("Text").gameObject.GetComponent<TextMeshPro>();
        if (text.color == Color.white || text.color == ColorHelper.FSColor)
            text.color = ColorHelper.FSColor;
        else
            text.color = new Color(0.45f, 0.45f, 0.65f);
    }

    private static void SetRoleAreaColors()
    {
        var roleArea = GameObject.Find("ROLES TAB").transform.FindChild("Scroller").FindChild("SliderInner");
        GameOptionsMenuPatch.SetColorForCat(
            roleArea.FindChild("ChancesTab").FindChild("CategoryHeaderMasked").gameObject, Color.green);
        GameOptionsMenuPatch.SetColorForCat(
            roleArea.FindChild("AdvancedTab").FindChild("CategoryHeaderMasked").gameObject, Color.blue);
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
    private static readonly List<Color32> normalbannercolors =
    [
        GetRoleColor(RoleTypes.Impostor),
        GetRoleColor(RoleTypes.Crewmate),
        Color.yellow,
        Color.green
    ];

    private static readonly List<Color32> hnSbannercolors =
    [
        GetRoleColor(RoleTypes.Crewmate),
        GetRoleColor(RoleTypes.Impostor),
        Palette.Purple,
        Color.green
    ];

    public static void Postfix()
    {
        var setArea = GameObject.Find("GAME SETTINGS TAB").transform.FindChild("Scroller").FindChild("SliderInner");
        Transform[] banners = setArea.GetComponentsInChildren<Transform>(true);
        if (IsNormalGame)
        {
            var headerindex = 0;
            var numindex = 0;
            var boxindex = 0;
            foreach (var banner in banners)
                if (banner.name == "CategoryHeaderMasked(Clone)")
                {
                    SetColorForCat(banner.gameObject, normalbannercolors[headerindex]);
                    headerindex++;
                }
                else if (banner.name.Contains("Num") || banner.name.Contains("Str"))
                {
                    Color color = numindex switch
                    {
                        <= 3 => normalbannercolors[0],
                        <= 5 => normalbannercolors[1],
                        <= 9 => normalbannercolors[2],
                        _ => normalbannercolors[3]
                    };
                    SetColorForSettingsOpt_StringAndNumber(banner.gameObject, color);
                    numindex++;
                }
                else if (banner.name.Contains("Checkbox"))
                {
                    Color color = boxindex <= 1 ? normalbannercolors[2] : normalbannercolors[3];
                    SetColorForSettingsOpt_Checkbox(banner.gameObject, color);
                    boxindex++;
                }
        }
        else
        {
            var headerindex = 0;
            var numindex = 0;
            var boxindex = 0;
            foreach (var banner in banners)
                if (banner.name == "CategoryHeaderMasked(Clone)")
                {
                    SetColorForCat(banner.gameObject, hnSbannercolors[headerindex]);
                    headerindex++;
                }
                else if (banner.name.Contains("Num") || banner.name.Contains("Str") || banner.name.Contains("Play"))
                {
                    Color color = numindex switch
                    {
                        <= 5 => hnSbannercolors[0],
                        <= 8 => hnSbannercolors[1],
                        <= 11 => hnSbannercolors[2],
                        _ => hnSbannercolors[3]
                    };
                    SetColorForSettingsOpt_StringAndNumber(banner.gameObject, color);
                    numindex++;
                }
                else if (banner.name.Contains("Checkbox"))
                {
                    Color color = boxindex <= 1 ? hnSbannercolors[0] : hnSbannercolors[2];
                    SetColorForSettingsOpt_Checkbox(banner.gameObject, color);
                    boxindex++;
                }
        }
    }

    internal static void SetColorForCat(GameObject obj, Color color)
    {
        if (!obj) return;
        obj.transform.FindChild("LabelSprite").gameObject.GetComponent<SpriteRenderer>().color =
            color.ShadeColor(0.18f);
        obj.transform.FindChild("DividerImage").gameObject.GetComponent<SpriteRenderer>().color =
            color.ShadeColor(0.18f);
    }

    private static void SetColorForSettingsOpt_StringAndNumber(GameObject obj, Color color)
    {
        obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color =
            color.ShadeColor(0.38f);
        obj.transform.FindChild("ValueBox").gameObject.GetComponent<SpriteRenderer>().color = color;
    }

    private static void SetColorForSettingsOpt_Checkbox(GameObject obj, Color color)
    {
        obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color =
            color.ShadeColor(0.38f);
        obj.transform.FindChild("Toggle").FindChild("InactiveSprite").gameObject.GetComponent<SpriteRenderer>().color =
            color;
    }
}

[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Update))]
internal class GameSettingMenuPatch
{
    private static GameObject _gamePresetButton;
    private static GameObject _gameSettingsButton;
    private static GameObject _roleSettingsButton;

    public static void Postfix()
    {
        try
        {
            var panel = GameObject.Find("LeftPanel");

            if (!_gamePresetButton) _gamePresetButton = panel.transform.FindChild("GamePresetButton").gameObject;

            if (!_gameSettingsButton) _gameSettingsButton = panel.transform.FindChild("GameSettingsButton").gameObject;

            if (!_roleSettingsButton && IsNormalGame)
                _roleSettingsButton = panel.transform.FindChild("RoleSettingsButton").gameObject;

            SetColor(_gamePresetButton, new Color32(205, 255, 253, 255));
            SetColor(_gameSettingsButton, new Color32(206, 205, 253, 255));
            SetColor(_roleSettingsButton, new Color32(185, 255, 181, 255));

            var ps = GameObject.Find("PanelSprite");
            ps.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.4f);
            ps.transform.FindChild("LeftSideTint").gameObject.GetComponent<SpriteRenderer>().color =
                new Color(0.1176f, 0.1176f, 0.1176f, 0.8f);
        }
        catch
        {
            /* ignored */
        }
    }

    private static void SetColor(GameObject obj, Color bgcolor)
    {
        if (!obj) return;
        obj.transform.FindChild("Highlight").gameObject.GetComponent<SpriteRenderer>().color = bgcolor;
        obj.transform.FindChild("Selected").gameObject.GetComponent<SpriteRenderer>().color = bgcolor;
        obj.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().color = bgcolor;
    }
}