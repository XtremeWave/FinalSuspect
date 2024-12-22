using System;
using AmongUs.GameOptions;
using HarmonyLib;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using FinalSuspect.Player;
using UnityEngine.ProBuilder;

namespace FinalSuspect.Patches;
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
    private static readonly Dictionary<string, GameObject> ButtonCache = new();

    public static void Postfix()
    {
        if (!XtremeGameData.GameStates.IsNormalGame) return;
        try
        {
            var header = GameObject.Find("HeaderButtons");
            CacheButtons(header);

            SetButtonColors();

            var roleArea = GameObject.Find("ROLES TAB").transform.FindChild("Scroller").FindChild("SliderInner");
            SetColorForCat(roleArea.FindChild("ChancesTab").FindChild("CategoryHeaderMasked").gameObject, Color.green);
            SetColorForCat(roleArea.FindChild("AdvancedTab").FindChild("CategoryHeaderMasked").gameObject, Color.blue);
        }
        catch { }
    }

    private static void CacheButtons(GameObject header)
    {
        var buttonNames = new[]
        {
            "AllButton", "EngineerButton", "GuardianAngelButton", "ScientistButton",
            "TrackerButton", "NoiseMakerButton", "ShapeShifterButton", "PhantomButton"
        };
        foreach (var buttonName in buttonNames)
        {
            if (!ButtonCache.ContainsKey(buttonName))
            {
                Transform child;
                switch (buttonName)
                {
                    case "AllButton":
                        child = header.transform.FindChild("AllButton");
                        break;
                    default:
                        child = header.transform.GetChild(Array.IndexOf(buttonNames, buttonName));
                        break;
                }
                ButtonCache[buttonName] = child.gameObject;
            }
        }
    }

    private static void SetButtonColors()
    {
        SetColor(ButtonCache["AllButton"], ColorHelper.ModColor32, ColorHelper.ModColor32);
        SetColor(ButtonCache["EngineerButton"], Utils.GetRoleColor(RoleTypes.Engineer), Utils.GetRoleColor(RoleTypes.Crewmate));
        SetColor(ButtonCache["GuardianAngelButton"], Utils.GetRoleColor(RoleTypes.GuardianAngel), Utils.GetRoleColor(RoleTypes.Crewmate));
        SetColor(ButtonCache["ScientistButton"], Utils.GetRoleColor(RoleTypes.Scientist), Utils.GetRoleColor(RoleTypes.Crewmate));
        SetColor(ButtonCache["TrackerButton"], Utils.GetRoleColor(RoleTypes.Tracker), Utils.GetRoleColor(RoleTypes.Crewmate));
        SetColor(ButtonCache["NoiseMakerButton"], Utils.GetRoleColor(RoleTypes.Noisemaker), Utils.GetRoleColor(RoleTypes.Crewmate));
        SetColor(ButtonCache["ShapeShifterButton"], Utils.GetRoleColor(RoleTypes.Shapeshifter), Utils.GetRoleColor(RoleTypes.Impostor));
        SetColor(ButtonCache["PhantomButton"], Utils.GetRoleColor(RoleTypes.Phantom), Utils.GetRoleColor(RoleTypes.Impostor));
    }

    private static void SetColor(GameObject obj, Color iconColor, Color backgroundColor)
    {
        obj.transform.FindChild("SelectedHighlight").gameObject.GetComponent<SpriteRenderer>().color = backgroundColor;
        obj.transform.FindChild("HoverHighlight").gameObject.GetComponent<SpriteRenderer>().color = backgroundColor; 
        obj.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().color = backgroundColor;
        obj.transform.FindChild("RoleIcon").gameObject.GetComponent<SpriteRenderer>().color = iconColor;

        var textColor = obj.transform.FindChild("Text").gameObject.GetComponent<TextMeshPro>().color;
        if (textColor == Color.white || textColor == ColorHelper.ModColor32)
        {
            obj.transform.FindChild("Text").gameObject.GetComponent<TextMeshPro>().color = ColorHelper.ModColor32;
        }
        else
        {
            obj.transform.FindChild("Text").gameObject.GetComponent<TextMeshPro>().color = new Color(0.45f, 0.45f, 0.65f);
        }
    }

    private static void SetColorForCat(GameObject cat, Color color)
    {
        cat.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.38f);
    }
}
[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
class NormalGameOptionsMenuPatch
{
    private static readonly Dictionary<string, GameObject> CategoryCache = new();
    private static readonly Dictionary<string, GameObject> SettingBannerCache = new();

    public static void Postfix()
    {
        if (!XtremeGameData.GameStates.IsNormalGame) return;
        try
        {
            var setArea = FindSettingsArea();
            Transform[] banners = setArea.GetComponentsInChildren<Transform>(true);

            CacheCategories(banners);
            CacheSettingBanners(banners);

            ApplyColorsToCategories();
            ApplyColorsToSettingBanners();
        }
        catch { }
    }

    private static void CacheCategories(Transform[] banners)
    {
        foreach (var banner in banners)
        {
            if (banner.name == "CategoryHeaderMasked(Clone)")
            {
                CategoryCache[banner.name] = banner.gameObject;
            }
        }
    }

    private static void CacheSettingBanners(Transform[] banners)
    {
        foreach (var banner in banners)
        {
            if (banner.name.Contains("GameOption"))
            {
                SettingBannerCache[banner.name] = banner.gameObject;
            }
        }
    }

    private static void ApplyColorsToCategories()
    {
        var categoryColors = new Dictionary<string, Color>
        {
            {"ImpostorCat", Utils.GetRoleColor(RoleTypes.Impostor)},
            {"CrewmateCat", Utils.GetRoleColor(RoleTypes.Crewmate)},
            {"MeetingCat", Color.yellow},
            {"TaskCat", Color.green}
        };

        foreach (var category in categoryColors)
        {
            SetColorForCat(CategoryCache[category.Key], category.Value);
        }
    }

    private static void ApplyColorsToSettingBanners()
    {
        var settingColors = new Dictionary<string, (Color color, bool isCheckbox)>
        {
            {"ImpostorSettingBanner", (Utils.GetRoleColor(RoleTypes.Impostor), false)},
            {"CrewmateSettingBanner", (Utils.GetRoleColor(RoleTypes.Crewmate), false)},
            {"MeetingSettingBanner", (Color.yellow, true)},
            {"TaskSettingBanner", (Color.green, false)}
        };

        foreach (var setting in settingColors)
        {
            for (int i = 1; i <= 5; i++)
            {
                string key = $"{setting.Key}{i}";
                if (SettingBannerCache.TryGetValue(key, out var banner))
                {
                    SetColorForSettingsOpt(banner, setting.Value.color, setting.Value.isCheckbox);
                }
            }
        }
    }

    private static Transform FindSettingsArea()
    {
        return GameObject.Find("GAME SETTINGS TAB").transform.FindChild("Scroller").FindChild("SliderInner");
    }

    private static void SetColorForCat(GameObject cat, Color color)
    {
        cat.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.38f);
    }

    private static void SetColorForSettingsOpt(GameObject obj, Color color, bool isCheckbox)
    {
        if (isCheckbox)
        {
            obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.38f);
            obj.transform.FindChild("Toggle").FindChild("InactiveSprite").gameObject.GetComponent<SpriteRenderer>().color = color;
        }
        else
        {
            obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.38f);
            obj.transform.FindChild("ValueBox").gameObject.GetComponent<SpriteRenderer>().color = color;
        }
    }
}[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
class HnSGameOptionsMenuPatch
{
    private static readonly Dictionary<string, GameObject> CategoryCache = new();
    private static readonly Dictionary<string, GameObject> SettingBannerCache = new();

    public static void Postfix()
    {
        if (XtremeGameData.GameStates.IsNormalGame) return;
        try
        {
            var setArea = FindSettingsArea();
            Transform[] banners = setArea.GetComponentsInChildren<Transform>(true);

            CacheCategories(banners);
            CacheSettingBanners(banners);

            ApplyColorsToCategories();
            ApplyColorsToSettingBanners();
        }
        catch { }
    }

    private static void CacheCategories(Transform[] banners)
    {
        foreach (var banner in banners)
        {
            if (banner.name == "CategoryHeaderMasked(Clone)")
            {
                CategoryCache[banner.name] = banner.gameObject;
            }
        }
    }

    private static void CacheSettingBanners(Transform[] banners)
    {
        foreach (var banner in banners)
        {
            if (banner.name.Contains("GameOption"))
            {
                SettingBannerCache[banner.name] = banner.gameObject;
            }
        }
    }

    private static void ApplyColorsToCategories()
    {
        var colors = new Dictionary<string, Color>
        {
            {"CrewmateCat", Utils.GetRoleColor(RoleTypes.Crewmate)},
            {"ImpostorCat", Utils.GetRoleColor(RoleTypes.Impostor)},
            {"LastHiddenCat", Palette.Purple},
            {"TaskCat", Color.green}
        };

        foreach (var category in colors)
        {
            SetColorForCat(CategoryCache[category.Key], category.Value);
        }
    }

    private static void ApplyColorsToSettingBanners()
    {
        var colors = new Dictionary<string, Color>
        {
            {"CrewmateSettingBanner", Utils.GetRoleColor(RoleTypes.Crewmate)},
            {"ImpostorSettingBanner", Utils.GetRoleColor(RoleTypes.Impostor)},
            {"LastHiddenSettingBanner", Palette.Purple},
            {"TaskSettingBanner", Color.green}
        };

        foreach (var color in colors)
        {
            for (int i = 1; i <= 8; i++)
            {
                string key = $"{color.Key}{i}";
                if (SettingBannerCache.ContainsKey(key))
                {
                    SetColorForSettingsOpt(SettingBannerCache[key], color.Value,
                        i > 4 && i != 5 && i != 8 && i != 12 && i != 15);
                }
            }
        }    
    }

    private static Transform FindSettingsArea()
    {
        return GameObject.Find("GAME SETTINGS TAB").transform.FindChild("Scroller").FindChild("SliderInner");
    }
    
    private static void SetColorForCat(GameObject obj, Color color)
    {
        if (obj == null) return;
        obj.transform.FindChild("LabelSprite").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.18f);
        obj.transform.FindChild("DividerImage").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.18f);
    }
    private static void SetColorForSettingsOpt(GameObject obj, Color color, bool isCheckbox)
    {
        if (isCheckbox)
        {
            obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.38f);
            obj.transform.FindChild("Toggle").FindChild("InactiveSprite").gameObject.GetComponent<SpriteRenderer>().color = color;
        }
        else
        {
            obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.38f);
            obj.transform.FindChild("ValueBox").gameObject.GetComponent<SpriteRenderer>().color = color;
        }
    }
}
[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Update))]
class GameSettingMenuPatch
{
    private static readonly Dictionary<string, GameObject> ButtonCache = new();

    public static void Postfix()
    {
        try
        {
            var panel = GameObject.Find("LeftPanel");
            CacheButtons(panel);

            SetButtonColors();

            var ps = GameObject.Find("PanelSprite");
            SetColorForPanelSprite(ps);
        }
        catch { }
    }

    private static void CacheButtons(GameObject panel)
    {
        var buttonNames = new[] { "GamePresetButton", "GameSettingsButton", "RoleSettingsButton" };
        foreach (var buttonName in buttonNames)
        {
            if (!ButtonCache.ContainsKey(buttonName) || buttonName == "RoleSettingsButton" && !XtremeGameData.GameStates.IsNormalGame)
            {
                ButtonCache[buttonName] = panel.transform.FindChild(buttonName)?.gameObject;
            }
        }
    }

    private static void SetButtonColors()
    {
        SetColor(ButtonCache["GamePresetButton"], new Color32(205, 255, 253, 255));
        SetColor(ButtonCache["GameSettingsButton"], new Color32(206, 205, 253, 255));
        if (ButtonCache["RoleSettingsButton"] != null)
        {
            SetColor(ButtonCache["RoleSettingsButton"], new Color32(185, 255, 181, 255));
        }
    }

    private static void SetColor(GameObject obj, Color color)
    {
        if (obj == null) return;
        obj.transform.FindChild("Highlight").GetComponent<SpriteRenderer>().color = color;
        obj.transform.FindChild("Selected").GetComponent<SpriteRenderer>().color = color;
        obj.transform.FindChild("Inactive").GetComponent<SpriteRenderer>().color = color;
    }

    private static void SetColorForPanelSprite(GameObject ps)
    {
        ps.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.4f);
        ps.transform.FindChild("LeftSideTint").GetComponent<SpriteRenderer>().color = new Color(0.1176f, 0.1176f, 0.1176f, 0.8f);
    }
}
