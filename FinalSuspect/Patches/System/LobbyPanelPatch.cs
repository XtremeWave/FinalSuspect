using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game;
using Il2CppSystem;
using TMPro;
using UnityEngine;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(LobbyInfoPane), nameof(LobbyInfoPane.Update))]
class LobbyInfoPanePatch
{

    static void Postfix()
    {
        var AspectSize = GameObject.Find("AspectSize");
        AspectSize.transform.FindChild("Background").gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.4f);
        if (XtremeGameData.GameStates.MapIsActive(MapNames.Dleks))
            AspectSize.transform.FindChild("MapImage").gameObject.GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite("DleksBanner-Wordart.png", 160f);
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
    private static readonly List<Color32> Normalbannercolors = new List<Color32>
    {
        Utils.GetRoleColor(RoleTypes.Impostor),
        Utils.GetRoleColor(RoleTypes.Crewmate),
        Color.yellow,
        Color.green
    };
    private static readonly List<Color32> HnSbannercolors = new List<Color32>
    {
        Utils.GetRoleColor(RoleTypes.Crewmate),
        Utils.GetRoleColor(RoleTypes.Impostor),
        Palette.Purple,
        Color.green
    };
    private static readonly List<Color32> rolecolors = new List<Color32>
    {
        Utils.GetRoleColor(RoleTypes.Engineer),
        Utils.GetRoleColor(RoleTypes.GuardianAngel),
        Utils.GetRoleColor(RoleTypes.Scientist),
        Utils.GetRoleColor(RoleTypes.Tracker),
        Utils.GetRoleColor(RoleTypes.Noisemaker),
        Utils.GetRoleColor(RoleTypes.Shapeshifter),
        Utils.GetRoleColor(RoleTypes.Phantom),
    };
    private static readonly List<Color32> rolecatcolors = new List<Color32>
    {
        Color.green,
        Color.blue
    };

    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Update)), HarmonyPostfix]
    static void Update()
    {
        var Area = GameObject.Find("MainArea").transform.FindChild("Scaler").FindChild("Scroller").FindChild("SliderInner");
        Transform[] banners = Area.GetComponentsInChildren<Transform>(true);
        
        if (XtremeGameData.GameStates.IsNormalGame)
        {
            #region 游戏设置
            if (Area.childCount == 21)
            {
                int catindex = 0;
                var bannerindex = 0;
                foreach (var banner in banners)
                {
                    if (banner.name == "CategoryHeaderMasked LongDivider(Clone)")
                    {
                        SetColorForCat(banner.gameObject, Normalbannercolors[catindex]);
                        catindex++;
                    }

                    if (banner.name == "ViewSettingsInfoPanel(Clone)")
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
                    if (banner.name == "CategoryHeaderMasked LongDivider(Clone)")
                    {
                        SetColorForCat(banner.gameObject, rolecatcolors[catindex]);
                        catindex++;
                    }

                    if (banner.name == "ViewSettingsInfoPanel_Role Variant(Clone)")
                    {
                        Color roleColor = bannerindex <= 4
                            ? Utils.GetRoleColor(RoleTypes.Crewmate)
                            : Utils.GetRoleColor(RoleTypes.Impostor);
                        SetColorForRolesBanner(banner.gameObject, rolecolors[bannerindex], roleColor);
                        if (banner.gameObject.transform.FindChild("LabelBackground").gameObject
                                .GetComponent<SpriteRenderer>().color != new Color(0.3f, 0.3f, 0.3f, 1))
                        {
                            enableroleindex.Add(bannerindex);
                        }

                        bannerindex++;
                    }

                }

                foreach (var banner in banners)
                {
                    if (banner.name == "AdvancedRoleViewPanel(Clone)")
                    {
                        var iconindex = enableroleindex.First();
                        Color roleColor = iconindex <= 4
                            ? Utils.GetRoleColor(RoleTypes.Crewmate)
                            : Utils.GetRoleColor(RoleTypes.Impostor);
                        SetColorForIcon(banner.gameObject, rolecolors[iconindex], roleColor);
                        enableroleindex.RemoveAt(0);
                    }
                }
            }

            #endregion
        }
        else
        {
            #region 游戏设置
            int catindex = 0;
            var bannerindex = 0;
            foreach (var banner in banners)
            {
                if (banner.name == "CategoryHeaderMasked LongDivider(Clone)")
                {
                    SetColorForCat(banner.gameObject, HnSbannercolors[catindex]);
                    catindex++;
                }
                if (banner.name == "ViewSettingsInfoPanel(Clone)")
                {
                    Color color;
                    if (bannerindex <= 7)
                        color = HnSbannercolors[0];
                    else if (bannerindex <= 10)
                        color = HnSbannercolors[1];
                    else if (bannerindex <= 15)
                        color = HnSbannercolors[2];
                    else 
                        color = HnSbannercolors[3];
                    SetColorForSettingsBanner(banner.gameObject, color);
                    bannerindex++;
                }
            }

            #endregion
        }

    }
    static void SetColorForRolesBanner(GameObject obj, Color iconcolor, Color bgcolor)
    {
        if (obj == null) return;
        if (obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color == new Color(0.3f, 0.3f, 0.3f, 1)) return;
        obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = bgcolor.ShadeColor(0.32f);
        obj.transform.FindChild("RoleIcon").gameObject.GetComponent<SpriteRenderer>().color = iconcolor;
    }

    static void SetColorForIcon(GameObject obj, Color iconcolor, Color bgcolor)
    {
        if (obj == null) return;
        var cat = obj.transform.FindChild("CategoryHeaderRoleVariant");
        cat.FindChild("LabelSprite").gameObject.GetComponent<SpriteRenderer>().color = bgcolor.ShadeColor(0.32f);
        cat.FindChild("Divider").gameObject.GetComponent<SpriteRenderer>().color = bgcolor.ShadeColor(0.32f);
        cat.FindChild("HeaderText").gameObject.GetComponent<TextMeshPro>().color = Color.white;
        cat.FindChild("Icon").gameObject.GetComponent<SpriteRenderer>().color = iconcolor;
        obj.ForEachChild((Action<GameObject>)SetColor);
        void SetColor(GameObject obj)
        {
            if (obj.name == "ViewSettingsInfoPanel(Clone)")
            {
                obj.transform.FindChild("Value").FindChild("Sprite").gameObject.GetComponent<SpriteRenderer>().color = iconcolor;
                obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = bgcolor.ShadeColor(0.38f);

            }
        }

    }
    static void SetColorForSettingsBanner(GameObject obj, Color color)
    {
        if (obj == null) return;
        obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.38f);
        obj.transform.FindChild("Value").FindChild("Sprite").gameObject.GetComponent<SpriteRenderer>().color = color;
    }
    static void SetColorForCat(GameObject obj, Color color)
    {
        if (obj == null) return;
        obj.transform.FindChild("LabelSprite").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.18f);
        obj.transform.FindChild("DividerImage").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.18f);
    }
}
