using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using FinalSuspect.Player;
using HarmonyLib;
using Il2CppSystem.Runtime.Remoting.Messaging;
using Innersloth.DebugTool;
using TMPro;
using UnityEngine;

namespace FinalSuspect.Patches;

[HarmonyPatch(typeof(LobbyInfoPane), nameof(LobbyInfoPane.Update))]
class LobbyInfoPanePatch
{

    static void Postfix()
    {
        var AspectSize = GameObject.Find("AspectSize");
        AspectSize.transform.FindChild("Background").gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.4f);
        if (XtremeGameData.GameStates.MapIsActive(MapNames.Dleks))
            AspectSize.transform.FindChild("MapImage").gameObject.GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite($"DleksBanner-Wordart.png", 160f);
    }

}
[HarmonyPatch]
class LobbyViewSettingsPanePatch
{
    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Awake)), HarmonyPostfix]
    static void Awake()
    {
        GameObject.Find("RulesPopOutWindow").transform.localPosition += Vector3.left * 0.4f;
    }
}

[HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Update))]
class NormalLobbyViewSettingsPanePatch
{
    private static readonly Dictionary<string, GameObject> CategoryCache = new();
    private static readonly Dictionary<string, GameObject> SettingBannerCache = new();
    private static readonly Dictionary<string, GameObject> RoleBannerCache = new();

    [HarmonyPostfix]
    static void Update()
    {
        if (!XtremeGameData.GameStates.IsNormalGame) return;

        var area = GameObject.Find("MainArea").transform.FindChild("Scaler").FindChild("Scroller").FindChild("SliderInner");
        if (area == null) return;

        CacheCategories(area);
        CacheSettingBanners(area);
        CacheRoleBanners(area);

        ApplyColorsToCategories();
        ApplyColorsToSettingBanners();
        ApplyColorsToRoleBanners();
    }

    private static void CacheCategories(Transform area)
    {
        foreach (Transform banner in area)
        {
            if (banner.name == "CategoryHeaderMasked LongDivider(Clone)")
            {
                CategoryCache[banner.name] = banner.gameObject;
            }
        }
    }

    private static void CacheSettingBanners(Transform area)
    {
        foreach (Transform banner in area)
        {
            if (banner.name == "ViewSettingsInfoPanel(Clone)")
            {
                SettingBannerCache[banner.name] = banner.gameObject;
            }
        }
    }

    private static void CacheRoleBanners(Transform area)
    {
        foreach (Transform banner in area)
        {
            if (banner.name == "ViewSettingsInfoPanel_Role Variant(Clone)" || banner.name == "AdvancedRoleViewPanel(Clone)")
            {
                RoleBannerCache[banner.name] = banner.gameObject;
            }
        }
    }

    private static void ApplyColorsToCategories()
    {
        if (CategoryCache.TryGetValue("CategoryHeaderMasked LongDivider(Clone)", out var impostorCat))
        {
            SetColorForCat(impostorCat, Utils.GetRoleColor(RoleTypes.Impostor));
        }
        // Add other categories as needed
    }

    private static void ApplyColorsToSettingBanners()
    {
        foreach (var banner in SettingBannerCache.Values)
        {
            if (banner.name.Contains("Impostor"))
            {
                SetColorForSettingsBanner(banner, Utils.GetRoleColor(RoleTypes.Impostor));
            }
            else if (banner.name.Contains("Crewmate"))
            {
                SetColorForSettingsBanner(banner, Utils.GetRoleColor(RoleTypes.Crewmate));
            }
            // Add other conditions as needed
        }
    }

    private static void ApplyColorsToRoleBanners()
    {
        foreach (var banner in RoleBannerCache.Values)
        {
            if (banner.name.Contains("Engineer"))
            {
                SetColorForRolesBanner(banner, Utils.GetRoleColor(RoleTypes.Engineer), Utils.GetRoleColor(RoleTypes.Crewmate));
            }
            // Add other role colors as needed
        }
    }

    public static void SetColorForCat(GameObject obj, Color color)
    {
        if (obj == null) return;
        obj.transform.Find("LabelSprite").GetComponent<SpriteRenderer>().color = color.ShadeColor(0.18f);
        obj.transform.Find("DividerImage").GetComponent<SpriteRenderer>().color = color.ShadeColor(0.18f);
    }

    public static void SetColorForSettingsBanner(GameObject obj, Color color)
    {
        if (obj == null) return;
        obj.transform.Find("LabelBackground").GetComponent<SpriteRenderer>().color = color.ShadeColor(0.38f);
        obj.transform.Find("Value").Find("Sprite").GetComponent<SpriteRenderer>().color = color;
    }

    public static void SetColorForRolesBanner(GameObject obj, Color iconcolor, Color bgcolor)
    {
        if (obj == null) return;
        obj.transform.Find("LabelBackground").GetComponent<SpriteRenderer>().color = bgcolor.ShadeColor(0.32f);
        obj.transform.Find("RoleIcon").GetComponent<SpriteRenderer>().color = iconcolor;
    }
}

[HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Update))]
class HnSLobbyViewSettingsPanePatch
{
    private static readonly Dictionary<string, GameObject> Cats = new();
    private static readonly Dictionary<string, GameObject> SettingBanners = new();

    [HarmonyPostfix]
    static void Update()
    {
        if (XtremeGameData.GameStates.IsNormalGame) return;

        var area = GameObject.Find("MainArea").transform.FindChild("Scaler").FindChild("Scroller").FindChild("SliderInner");
        if (area == null) return;

        CacheGameObjects(area);
        ApplyColors();
    }

    private static void CacheGameObjects(Transform area)
    {
        CacheCats(area);
        CacheSettingBanners(area);
    }

    private static void CacheCats(Transform area)
    {
        foreach (Transform child in area)
        {
            if (child.name == "CategoryHeaderMasked LongDivider(Clone)")
            {
                Cats[child.name] = child.gameObject;
            }
        }
    }

    private static void CacheSettingBanners(Transform area)
    {
        foreach (Transform child in area)
        {
            if (child.name == "ViewSettingsInfoPanel(Clone)")
            {
                SettingBanners[child.name] = child.gameObject;
            }
        }
    }

    private static void ApplyColors()
    {
        SetColorForCat(Cats["CategoryHeaderMasked LongDivider(Clone)"], Utils.GetRoleColor(RoleTypes.Crewmate));
        SetColorForCat(Cats["CategoryHeaderMasked LongDivider(Clone)"], Utils.GetRoleColor(RoleTypes.Impostor));
        SetColorForCat(Cats["CategoryHeaderMasked LongDivider(Clone)"], Palette.Purple);
        SetColorForCat(Cats["CategoryHeaderMasked LongDivider(Clone)"], Color.green);

        for (int i = 0; i < 8; i++)
        {
            var bannerName = $"CrewmateSettingBanner{i + 1}";
            SetColorForSettingsBanner(SettingBanners[$"ViewSettingsInfoPanel(Clone)"], Utils.GetRoleColor(RoleTypes.Crewmate));
        }

        for (int i = 0; i < 3; i++)
        {
            var bannerName = $"ImpostorSettingBanner{i + 1}";
            SetColorForSettingsBanner(SettingBanners[$"ViewSettingsInfoPanel(Clone)"], Utils.GetRoleColor(RoleTypes.Impostor));
        }

        for (int i = 0; i < 5; i++)
        {
            var bannerName = $"LastHiddenSettingBanner{i + 1}";
            SetColorForSettingsBanner(SettingBanners[$"ViewSettingsInfoPanel(Clone)"], Palette.Purple);
        }

        for (int i = 0; i < 3; i++)
        {
            var bannerName = $"TaskSettingBanner{i + 1}";
            SetColorForSettingsBanner(SettingBanners[$"ViewSettingsInfoPanel(Clone)"], Color.green);
        }
    }

    public static void SetColorForCat(GameObject obj, Color color)
    {
        if (obj == null) return;
        obj.transform.FindChild("LabelSprite").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.18f);
        obj.transform.FindChild("DividerImage").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.18f);
    }

    public static void SetColorForSettingsBanner(GameObject obj, Color color)
    {
        if (obj == null) return;
        obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.38f);
        obj.transform.FindChild("Value").FindChild("Sprite").gameObject.GetComponent<SpriteRenderer>().color = color;
    }
}
