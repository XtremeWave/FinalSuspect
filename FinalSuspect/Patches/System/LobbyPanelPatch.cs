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
        trans.FindChild("ButtonSettingsHeader").gameObject.GetComponent<TextMeshPro>().text +=
            $" - {GetString("PressF2ToHidePane")}";
        DestroyableSingleton<LobbyInfoPane>.Instance.gameObject.GetComponent<AspectPosition>().DistanceFromEdge +=
            Vector3.forward * -30;
    }

    public static void Postfix()
    {
        var AspectSize = GameObject.Find("AspectSize");
        AspectSize.transform.FindChild("Background").gameObject.GetComponent<SpriteRenderer>().color =
            new Color(1, 1, 1, 0.4f);
        if (MapIsActive(MapNames.Dleks))
            AspectSize.transform.FindChild("MapImage").gameObject.GetComponent<SpriteRenderer>().sprite =
                LoadSprite("DleksBanner-Wordart.png", 160f);
    }
}

[HarmonyPatch]
internal class LobbyViewSettingsPanePatch
{
    private static readonly List<Color32> Normalbannercolors =
    [
        GetRoleColor(RoleTypes.Impostor),
        GetRoleColor(RoleTypes.Crewmate),
        Color.yellow,
        Color.green
    ];

    private static readonly List<Color32> HnSbannercolors =
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

    private static readonly List<Color32> rolecatcolors =
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
            var Area = GameObject.Find("MainArea").transform.FindChild("Scaler").FindChild("Scroller")
                .FindChild("SliderInner");
            Transform[] banners = Area.GetComponentsInChildren<Transform>(true);

            if (IsNormalGame)
            {
                #region 游戏设置

                if (Area.childCount == 21)
                {
                    var catindex = 0;
                    var bannerindex = 0;
                    foreach (var banner in banners)
                    {
                        switch (banner.name)
                        {
                            case "CategoryHeaderMasked LongDivider(Clone)":
                                SetColorForCat(banner.gameObject, Normalbannercolors[catindex]);
                                catindex++;
                                break;
                            case "ViewSettingsInfoPanel(Clone)":
                            {
                                Color color;
                                if (bannerindex <= 3)
                                    color = Normalbannercolors[0];
                                else if (bannerindex <= 5)
                                    color = Normalbannercolors[1];
                                else if (bannerindex <= 11)
                                    color = Normalbannercolors[2];
                                else
                                    color = Normalbannercolors[3];
                                SetColorForSettingsBanner(banner.gameObject, color);
                                bannerindex++;
                                break;
                            }
                        }
                    }
                }

                #endregion

                #region 职业详细设定

                else
                {
                    var catindex = 0;
                    var bannerindex = 0;
                    var enableroleindex = new List<int>();
                    foreach (var banner in banners)
                    {
                        switch (banner.name)
                        {
                            case "CategoryHeaderMasked LongDivider(Clone)":
                                SetColorForCat(banner.gameObject, rolecatcolors[catindex]);
                                catindex++;
                                break;
                            case "ViewSettingsInfoPanel_Role Variant(Clone)":
                            {
                                var roleColor = bannerindex <= 4
                                    ? GetRoleColor(RoleTypes.Crewmate)
                                    : GetRoleColor(RoleTypes.Impostor);
                                SetColorForRolesBanner(banner.gameObject, rolecolors[bannerindex], roleColor);
                                if (banner.gameObject.transform.FindChild("LabelBackground").gameObject
                                        .GetComponent<SpriteRenderer>().color != new Color(0.3f, 0.3f, 0.3f, 1))
                                    enableroleindex.Add(bannerindex);

                                bannerindex++;
                                break;
                            }
                        }
                    }

                    foreach (var banner in banners)
                        if (banner.name == "AdvancedRoleViewPanel(Clone)")
                        {
                            var iconindex = enableroleindex.First();
                            var roleColor = iconindex <= 4
                                ? GetRoleColor(RoleTypes.Crewmate)
                                : GetRoleColor(RoleTypes.Impostor);
                            SetColorForIcon(banner.gameObject, rolecolors[iconindex], roleColor);
                            enableroleindex.RemoveAt(0);
                        }
                }

                #endregion
            }
            else
            {
                #region 游戏设置

                var catindex = 0;
                var bannerindex = 0;
                foreach (var banner in banners)
                {
                    switch (banner.name)
                    {
                        case "CategoryHeaderMasked LongDivider(Clone)":
                            SetColorForCat(banner.gameObject, HnSbannercolors[catindex]);
                            catindex++;
                            break;
                        case "ViewSettingsInfoPanel(Clone)":
                        {
                            Color color = bannerindex switch
                            {
                                <= 7 => HnSbannercolors[0],
                                <= 10 => HnSbannercolors[1],
                                <= 15 => HnSbannercolors[2],
                                _ => HnSbannercolors[3]
                            };
                            SetColorForSettingsBanner(banner.gameObject, color);
                            bannerindex++;
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