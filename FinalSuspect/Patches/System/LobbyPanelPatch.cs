using AmongUs.GameOptions;
using FinalSuspect.Attributes;
using FinalSuspect.Helpers;
using Il2CppSystem;
using TMPro;
using UnityEngine;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(LobbyInfoPane), nameof(LobbyInfoPane.Update))]
internal class LobbyInfoPaneUpdatePatch
{
    [GameModuleInitializer]
    public static void Init()
    {
        var trans = DestroyableSingleton<LobbyInfoPane>.Instance.transform.FindChild("AspectSize")
            .FindChild("GameSettingsButtons");
        trans.FindChild("Host Buttons").gameObject.SetActive(false);
        trans.FindChild("Client Buttons").gameObject.SetActive(true);
        var header = trans.FindChild("ButtonSettingsHeader").gameObject;
        header.transform.localPosition =
            new Vector3(-0.282f, header.transform.localPosition.y, header.transform.localPosition.z);
        var tmp = header.GetComponent<TextMeshPro>();
        tmp.text += $" - {GetString("PressF2ToHidePane")}";
        var rect = header.gameObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(3f, rect.sizeDelta.y);

        DestroyableSingleton<LobbyInfoPane>.Instance.gameObject.GetComponent<AspectPosition>().DistanceFromEdge +=
            Vector3.forward * -60;
    }

    public static void Postfix()
    {
        var aspectSize = GameObject.Find("AspectSize");
        aspectSize.transform.FindChild("Background").gameObject.GetComponent<SpriteRenderer>().color =
            new Color(1, 1, 1, 0.4f);
        if (MapIsActive(MapNames.Dleks))
            aspectSize.transform.FindChild("MapImage").gameObject.GetComponent<SpriteRenderer>().sprite =
                LoadSprite("DleksBanner-Wordart.png", 160f);
    }
}

[HarmonyPatch]
internal class LobbyViewSettingsPanePatch
{
    private static readonly List<Color32> normalBannerColors =
    [
        GetRoleColor(RoleTypes.Impostor),
        GetRoleColor(RoleTypes.Crewmate),
        Color.yellow,
        Color.green
    ];

    private static readonly List<Color32> hnsBannerColors =
    [
        GetRoleColor(RoleTypes.Crewmate),
        GetRoleColor(RoleTypes.Impostor),
        Palette.Purple,
        Color.green
    ];

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

    private static readonly List<Color32> roleCatColors =
    [
        Color.green,
        Color.blue
    ];

    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Awake))]
    [HarmonyPostfix]
    private static void Awake()
    {
        GameObject.Find("RulesPopOutWindow").transform.localPosition += Vector3.left * 0.4f;
    }

    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Update))]
    [HarmonyPostfix]
    private static void Update()
    {
        try
        {
            var area = GameObject.Find("MainArea").transform.FindChild("Scaler").FindChild("Scroller")
                .FindChild("SliderInner");
            Transform[] banners = area.GetComponentsInChildren<Transform>(true);

            if (IsNormalGame)
            {
                #region 游戏设置

                if (area.childCount == 21)
                {
                    var catIndex = 0;
                    var bannerIndex = 0;
                    foreach (var banner in banners)
                    {
                        switch (banner.name)
                        {
                            case "CategoryHeaderMasked LongDivider(Clone)":
                                SetColorForCat(banner.gameObject, normalBannerColors[catIndex]);
                                catIndex++;
                                break;
                            case "ViewSettingsInfoPanel(Clone)":
                            {
                                Color color = bannerIndex switch
                                {
                                    <= 3 => normalBannerColors[0],
                                    <= 5 => normalBannerColors[1],
                                    <= 11 => normalBannerColors[2],
                                    _ => normalBannerColors[3]
                                };
                                SetColorForSettingsBanner(banner.gameObject, color);
                                bannerIndex++;
                                break;
                            }
                        }
                    }
                }

                #endregion

                #region 职业详细设定

                else
                {
                    var catIndex = 0;
                    var bannerIndex = 0;
                    var enableRoleIndex = new List<int>();
                    foreach (var banner in banners)
                    {
                        switch (banner.name)
                        {
                            case "CategoryHeaderMasked LongDivider(Clone)":
                                SetColorForCat(banner.gameObject, roleCatColors[catIndex]);
                                catIndex++;
                                break;
                            case "ViewSettingsInfoPanel_Role Variant(Clone)":
                            {
                                var roleColor = bannerIndex <= 4
                                    ? GetRoleColor(RoleTypes.Crewmate)
                                    : GetRoleColor(RoleTypes.Impostor);
                                SetColorForRolesBanner(banner.gameObject, rolecolors[bannerIndex], roleColor);
                                if (banner.gameObject.transform.FindChild("LabelBackground").gameObject
                                        .GetComponent<SpriteRenderer>().color != new Color(0.3f, 0.3f, 0.3f, 1))
                                    enableRoleIndex.Add(bannerIndex);

                                bannerIndex++;
                                break;
                            }
                        }
                    }

                    foreach (var banner in banners)
                        if (banner.name == "AdvancedRoleViewPanel(Clone)")
                        {
                            var iconIndex = enableRoleIndex.First();
                            var roleColor = iconIndex <= 4
                                ? GetRoleColor(RoleTypes.Crewmate)
                                : GetRoleColor(RoleTypes.Impostor);
                            SetColorForIcon(banner.gameObject, rolecolors[iconIndex], roleColor);
                            enableRoleIndex.RemoveAt(0);
                        }
                }

                #endregion
            }
            else
            {
                #region 游戏设置

                var catIndex = 0;
                var bannerIndex = 0;
                foreach (var banner in banners)
                {
                    switch (banner.name)
                    {
                        case "CategoryHeaderMasked LongDivider(Clone)":
                            SetColorForCat(banner.gameObject, hnsBannerColors[catIndex]);
                            catIndex++;
                            break;
                        case "ViewSettingsInfoPanel(Clone)":
                        {
                            Color color = bannerIndex switch
                            {
                                <= 7 => hnsBannerColors[0],
                                <= 10 => hnsBannerColors[1],
                                <= 15 => hnsBannerColors[2],
                                _ => hnsBannerColors[3]
                            };
                            SetColorForSettingsBanner(banner.gameObject, color);
                            bannerIndex++;
                            break;
                        }
                    }
                }

                #endregion
            }
        }
        catch
        {
            /* ignored */
        }
    }

    private static void SetColorForRolesBanner(GameObject obj, Color iconcolor, Color bgcolor)
    {
        if (!obj) return;
        if (obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color ==
            new Color(0.3f, 0.3f, 0.3f, 1)) return;
        obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color =
            bgcolor.ShadeColor(0.32f);
        obj.transform.FindChild("RoleIcon").gameObject.GetComponent<SpriteRenderer>().color = iconcolor;
    }

    private static void SetColorForIcon(GameObject obj, Color iconcolor, Color bgcolor)
    {
        if (!obj) return;
        var cat = obj.transform.FindChild("CategoryHeaderRoleVariant");
        cat.FindChild("LabelSprite").gameObject.GetComponent<SpriteRenderer>().color = bgcolor.ShadeColor(0.32f);
        cat.FindChild("Divider").gameObject.GetComponent<SpriteRenderer>().color = bgcolor.ShadeColor(0.32f);
        cat.FindChild("HeaderText").gameObject.GetComponent<TextMeshPro>().color = Color.white;
        cat.FindChild("Icon").gameObject.GetComponent<SpriteRenderer>().color = iconcolor;
        obj.ForEachChild((Action<GameObject>)SetColor);
        return;

        void SetColor(GameObject _obj)
        {
            if (_obj.name != "ViewSettingsInfoPanel(Clone)") return;
            _obj.transform.FindChild("Value").FindChild("Sprite").gameObject.GetComponent<SpriteRenderer>().color =
                iconcolor;
            _obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color =
                bgcolor.ShadeColor(0.38f);
        }
    }

    private static void SetColorForSettingsBanner(GameObject obj, Color color)
    {
        if (!obj) return;
        obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color =
            color.ShadeColor(0.38f);
        obj.transform.FindChild("Value").FindChild("Sprite").gameObject.GetComponent<SpriteRenderer>().color = color;
    }

    private static void SetColorForCat(GameObject obj, Color color)
    {
        if (!obj) return;
        obj.transform.FindChild("LabelSprite").gameObject.GetComponent<SpriteRenderer>().color =
            color.ShadeColor(0.18f);
        obj.transform.FindChild("DividerImage").gameObject.GetComponent<SpriteRenderer>().color =
            color.ShadeColor(0.18f);
    }
}

/*
//不屎山更稳定，但是代码量大得多且还需要额外写Update
[HarmonyPatch(typeof(LobbyViewSettingsPane))]
 internal class LobbyViewSettingsPanePatch
 {
     private static readonly List<Color> normalBannerColors =
     [
         GetRoleColor(RoleTypes.Impostor),
         GetRoleColor(RoleTypes.Crewmate),
         Color.yellow,
         Color.green
     ];

     private static readonly List<Color> hnsBannerColors =
     [
         GetRoleColor(RoleTypes.Crewmate),
         GetRoleColor(RoleTypes.Impostor),
         Palette.Purple,
         Color.green
     ];

     private static readonly List<Color> roleTabCatColors =
     [
         Color.green,
         Color.blue
     ];

     [HarmonyPatch(nameof(LobbyViewSettingsPane.Awake))]
     [HarmonyPostfix]
     private static void Awake()
     {
         GameObject.Find("RulesPopOutWindow").transform.localPosition += Vector3.left * 0.4f;
     }

     private static void SetColorForRolesBanner(GameObject obj)
     {
         if (!obj || obj.TryGetComponent<AdvancedRoleViewPanel>(out _)) return;
         var (_role, _disable) = GetRole(obj);
         if (_role == null || _disable == null) return;
         var role = _role.Value;
         var disable = _disable.Value;
         var roleColor = GetRoleColor(role);
         var teamColor = role.IsImpostor() ? GetRoleColor(RoleTypes.Impostor) : GetRoleColor(RoleTypes.Crewmate);

         var bgColor = disable ? Palette.DisabledGrey : teamColor;
         var valueColor = disable ? Palette.DisabledGrey : roleColor;

         obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = bgColor.ShadeColor(0.32f);
         obj.transform.FindChild("RoleIcon").gameObject.GetComponent<SpriteRenderer>().color = roleColor;
         obj.transform.FindChild("Value").FindChild("Sprite").gameObject.GetComponent<SpriteRenderer>().color = valueColor;
     }

     private static void SetColorForIcon(GameObject obj)
     {
         if (!obj || !obj.TryGetComponent<AdvancedRoleViewPanel>(out _)) return;
         var (_role, _disable) = GetRole(obj);
         if (_role == null || _disable == null) return;

         var role = _role.Value;

         var roleColor = GetRoleColor(role);
         var teamColor = role.IsImpostor() ? GetRoleColor(RoleTypes.Impostor) : GetRoleColor(RoleTypes.Crewmate);
         var bgColor = teamColor.ShadeColor(0.32f);
         var cat = obj.transform.FindChild("CategoryHeaderRoleVariant");
         cat.FindChild("LabelSprite").gameObject.GetComponent<SpriteRenderer>().color =
             cat.FindChild("Divider").gameObject.GetComponent<SpriteRenderer>().color = bgColor;
         var tmp = cat.FindChild("HeaderText").gameObject.GetComponent<TextMeshPro>();
         tmp.color = cat.FindChild("Icon").gameObject.GetComponent<SpriteRenderer>().color = roleColor;
         tmp.SetOutlineColor(Color.black);
         tmp.SetOutlineThickness(0.1f);
         obj.ForEachChild((Action<GameObject>)SetColor);
         return;

         void SetColor(GameObject _obj)
         {
             if (_obj.TryGetComponent<ViewSettingsInfoPanel>(out _)) return;
             _obj.transform.FindChild("Value").FindChild("Sprite").gameObject.GetComponent<SpriteRenderer>().color = roleColor;
             _obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = bgColor;
         }
     }

     private static void SetColorForSettingsBanner(GameObject obj, Color color)
     {
         if (!obj || !obj.TryGetComponent<ViewSettingsInfoPanel>(out _)) return;
         obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.38f);
         obj.transform.FindChild("Value").FindChild("Sprite").gameObject.GetComponent<SpriteRenderer>().color = color;
     }

     private static void SetColorForCat(GameObject obj, List<Color> colors, ref int index)
     {
         if (!obj) return;
         if (!obj.TryGetComponent<CategoryHeaderMasked>(out _) || obj.TryGetComponent<CategoryHeaderRoleVariant>(out _)) return;
         index++;
         var color = colors[index - 1];
         obj.transform.FindChild("LabelSprite").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.18f);
         try
         {
             obj.transform.FindChild("DividerImage").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.18f);
         }
         catch
         {
             /* ignored
         }

     }

     [HarmonyPatch(nameof(LobbyViewSettingsPane.DrawNormalTab))]
     [HarmonyPostfix]
     private static void DrawNormalTab_Postfix(LobbyViewSettingsPane __instance)
     {
         var colorList = IsNormalGame ? normalBannerColors : hnsBannerColors;
         var index = 0;
         foreach (var item in __instance.settingsInfo)
         {
             SetColorForCat(item, colorList, ref index);
             SetColorForSettingsBanner(item, colorList[index - 1]);
         }
     }

     [HarmonyPatch(nameof(LobbyViewSettingsPane.DrawRolesTab))]
     [HarmonyPostfix]
     private static void DrawRolesTab_Postfix(LobbyViewSettingsPane __instance)
     {
         var index = 0;
         foreach (var item in __instance.settingsInfo)
         {
             SetColorForCat(item, roleTabCatColors,ref index);
             SetColorForRolesBanner(item);
             SetColorForIcon(item);
         }
     }
     private static (RoleTypes?, bool?) GetRole(GameObject item)
     {
         var allRoles = GameManager.Instance.GameSettingsList.AllRoles.ToArray().ToArray().ToList();
         RoleTypes role;
         if (item.TryGetComponent(out AdvancedRoleViewPanel advancedRoleViewPanel))
         {
             role = allRoles.FirstOrDefault(x => x.Role.RoleIconSolid == advancedRoleViewPanel.header.icon.sprite)!.Role
                 .Role;
         }
         else if (item.TryGetComponent(out ViewSettingsInfoPanelRoleVariant viewSettingsInfoPanelRoleVariant))
         {
             role = allRoles.FirstOrDefault(x =>
                 x.Role.RoleIconSolid == viewSettingsInfoPanelRoleVariant.iconSprite.sprite)!.Role.Role;
         }
         else return (null, null);

         var numPerGame = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(role);
         return (role, numPerGame == 0);
     }
 }

 */