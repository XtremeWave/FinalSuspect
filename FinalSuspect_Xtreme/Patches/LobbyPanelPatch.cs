using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppSystem.Runtime.Remoting.Messaging;
using Innersloth.DebugTool;
using TMPro;
using UnityEngine;

namespace FinalSuspect_Xtreme.Patches;

[HarmonyPatch(typeof(LobbyInfoPane), nameof(LobbyInfoPane.Update))]
class LobbyInfoPanePatch
{

    static void Postfix()
    {
        var AspectSize = GameObject.Find("AspectSize");
        AspectSize.transform.FindChild("Background").gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.4f);
        if (XtremeGameData.GameStates.DleksIsActive)
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
[HarmonyPatch]
class NormalLobbyViewSettingsPanePatch
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

    static GameObject RoleAmountCat = null;
    static GameObject RoleSettingsCat = null;

    static GameObject EngineerBanner = null;
    static GameObject GuardianAngelBanner = null;
    static GameObject ScientistBanner = null;
    static GameObject TrackerBanner = null;
    static GameObject NoiseMakerBanner = null;
    static GameObject ShapeShifterBanner = null;
    static GameObject PhantomBanner = null;

    static GameObject EngineerIcon = null;
    static GameObject GuardianAngelIcon = null;
    static GameObject ScientistIcon = null;
    static GameObject TrackerIcon = null;
    static GameObject NoiseMakerIcon = null;
    static GameObject ShapeShifterIcon = null;
    static GameObject PhantomIcon = null;
    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Update)), HarmonyPostfix]
    static void Update()
    {
        if (!XtremeGameData.GameStates.IsNormalGame) return;

        var Area = GameObject.Find("MainArea").transform.FindChild("Scaler").FindChild("Scroller").FindChild("SliderInner");
        Transform[] Banners = Area.GetComponentsInChildren<Transform>(true);

        #region 游戏设置
        if (Area.childCount == 21)
        {
            List<GameObject> Cats = new();

            foreach (var Banner in Banners)
            {
                if (Banner.name == "CategoryHeaderMasked LongDivider(Clone)")
                {
                    Cats.Add(Banner.gameObject);
                }
            }

            if (Cats.Count < 4) return;
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


            List<GameObject> ViewSettingsInfoPanel = new();

            foreach (var Banner in Banners)
            {
                if (Banner.name == "ViewSettingsInfoPanel(Clone)")
                {
                    ViewSettingsInfoPanel.Add(Banner.gameObject);
                }
            }

            if (ViewSettingsInfoPanel.Count < 17) return;
            if (ImpostorSettingBanner1 == null)
            {
                ImpostorSettingBanner1 = ViewSettingsInfoPanel[0];
            }
            if (ImpostorSettingBanner2 == null)
            {
                ImpostorSettingBanner2 = ViewSettingsInfoPanel[1];
            }
            if (ImpostorSettingBanner3 == null)
            {
                ImpostorSettingBanner3 = ViewSettingsInfoPanel[2];
            }
            if (ImpostorSettingBanner4 == null)
            {
                ImpostorSettingBanner4 = ViewSettingsInfoPanel[3];
            }
            if (CrewmateSettingBanner1 == null)
            {
                CrewmateSettingBanner1 = ViewSettingsInfoPanel[4];
            }
            if (CrewmateSettingBanner2 == null)
            {
                CrewmateSettingBanner2 = ViewSettingsInfoPanel[5];
            }
            if (MeetingSettingBanner1 == null)
            {
                MeetingSettingBanner1 = ViewSettingsInfoPanel[6];
            }
            if (MeetingSettingBanner2 == null)
            {
                MeetingSettingBanner2 = ViewSettingsInfoPanel[7];
            }
            if (MeetingSettingBanner3 == null)
            {
                MeetingSettingBanner3 = ViewSettingsInfoPanel[8];
            }
            if (MeetingSettingBanner4 == null)
            {
                MeetingSettingBanner4 = ViewSettingsInfoPanel[9];
            }
            if (MeetingSettingBanner5 == null)
            {
                MeetingSettingBanner5 = ViewSettingsInfoPanel[10];
            }
            if (MeetingSettingBanner6 == null)
            {
                MeetingSettingBanner6 = ViewSettingsInfoPanel[11];
            }
            if (TaskSettingBanner1 == null)
            {
                TaskSettingBanner1 = ViewSettingsInfoPanel[12];
            }
            if (TaskSettingBanner2 == null)
            {
                TaskSettingBanner2 = ViewSettingsInfoPanel[13];
            }
            if (TaskSettingBanner3 == null)
            {
                TaskSettingBanner3 = ViewSettingsInfoPanel[14];
            }
            if (TaskSettingBanner4 == null)
            {
                TaskSettingBanner4 = ViewSettingsInfoPanel[15];
            }
            if (TaskSettingBanner5 == null)
            {
                TaskSettingBanner5 = ViewSettingsInfoPanel[16];
            }

            SetColorForCat(ImpostorCat, Utils.GetRoleColor(RoleTypes.Impostor));
            SetColorForCat(CrewmateCat, Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForCat(MeetingCat, Color.yellow);
            SetColorForCat(TaskCat, Color.green);

            SetColorForSettingsBanner(ImpostorSettingBanner1, Utils.GetRoleColor(RoleTypes.Impostor));
            SetColorForSettingsBanner(ImpostorSettingBanner2, Utils.GetRoleColor(RoleTypes.Impostor));
            SetColorForSettingsBanner(ImpostorSettingBanner3, Utils.GetRoleColor(RoleTypes.Impostor));
            SetColorForSettingsBanner(ImpostorSettingBanner4, Utils.GetRoleColor(RoleTypes.Impostor));

            SetColorForSettingsBanner(CrewmateSettingBanner1, Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForSettingsBanner(CrewmateSettingBanner2, Utils.GetRoleColor(RoleTypes.Crewmate));

            SetColorForSettingsBanner(MeetingSettingBanner1, Color.yellow);
            SetColorForSettingsBanner(MeetingSettingBanner2, Color.yellow);
            SetColorForSettingsBanner(MeetingSettingBanner3, Color.yellow);
            SetColorForSettingsBanner(MeetingSettingBanner4, Color.yellow);
            SetColorForSettingsBanner(MeetingSettingBanner5, Color.yellow);
            SetColorForSettingsBanner(MeetingSettingBanner6, Color.yellow);

            SetColorForSettingsBanner(TaskSettingBanner1, Color.green);
            SetColorForSettingsBanner(TaskSettingBanner2, Color.green);
            SetColorForSettingsBanner(TaskSettingBanner3, Color.green);
            SetColorForSettingsBanner(TaskSettingBanner4, Color.green);
            SetColorForSettingsBanner(TaskSettingBanner5, Color.green);


        }
        #endregion
        #region 职业详细设定
        else
        {
            List<GameObject> Cats = new();

            foreach (var Banner in Banners)
            {
                if (Banner.name == "CategoryHeaderMasked LongDivider(Clone)")
                {
                    Cats.Add(Banner.gameObject);
                }
            }
            if (RoleAmountCat == null)
            {
                RoleAmountCat = Cats[0];
            }

            List<GameObject> RoleSettingsBanner = new();

            foreach (var Banner in Banners)
            {
                if (Banner.name == "ViewSettingsInfoPanel_Role Variant(Clone)")
                {
                    RoleSettingsBanner.Add(Banner.gameObject);
                }
            }

            if (EngineerBanner == null)
            {
                EngineerBanner = RoleSettingsBanner[0];
            }
            if (GuardianAngelBanner == null)
            {
                GuardianAngelBanner = RoleSettingsBanner[1];
            }
            if (ScientistBanner == null)
            {
                ScientistBanner = RoleSettingsBanner[2];
            }
            if (TrackerBanner == null)
            {
                TrackerBanner = RoleSettingsBanner[3];
            }
            if (NoiseMakerBanner == null)
            {
                NoiseMakerBanner = RoleSettingsBanner[4];
            }
            if (ShapeShifterBanner == null)
            {
                ShapeShifterBanner = RoleSettingsBanner[5];
            }
            if (PhantomBanner == null)
            {
                PhantomBanner = RoleSettingsBanner[6];
            }
            



            List<GameObject> AdvancedRoleSettingsBanner = new();

            foreach (var Banner in Banners)
            {
                if (Banner.name == "AdvancedRoleViewPanel(Clone)")
                {
                    AdvancedRoleSettingsBanner.Add(Banner.gameObject);
                }
            }

            for (int i = 0; i < AdvancedRoleSettingsBanner.Count; i++)
            {
                if (EngineerIcon == null && EngineerBanner.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color != new Color(0.3f, 0.3f, 0.3f, 1))
                {
                    EngineerIcon = AdvancedRoleSettingsBanner[i];
                    continue;
                }
                if (GuardianAngelIcon == null && GuardianAngelBanner.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color != new Color(0.3f, 0.3f, 0.3f, 1))
                {
                    GuardianAngelIcon = AdvancedRoleSettingsBanner[i];
                    continue;
                }
                if (ScientistIcon == null && ScientistBanner.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color != new Color(0.3f, 0.3f, 0.3f, 1))
                {
                    ScientistIcon = AdvancedRoleSettingsBanner[i];
                    continue;
                }
                if (TrackerIcon == null && TrackerBanner.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color != new Color(0.3f, 0.3f, 0.3f, 1))
                {
                    TrackerIcon = AdvancedRoleSettingsBanner[i];
                    continue;
                }
                if (NoiseMakerIcon == null && NoiseMakerBanner.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color != new Color(0.3f, 0.3f, 0.3f, 1))
                {
                    NoiseMakerIcon = AdvancedRoleSettingsBanner[i];
                    continue;
                }
                if (ShapeShifterIcon == null && ShapeShifterBanner.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color != new Color(0.3f, 0.3f, 0.3f, 1))
                {
                    ShapeShifterIcon = AdvancedRoleSettingsBanner[i];
                    continue;
                }
                if (PhantomIcon == null && PhantomBanner.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color != new Color(0.3f, 0.3f, 0.3f, 1))
                {
                    PhantomIcon = AdvancedRoleSettingsBanner[i];
                    continue;
                }
            }

            if (RoleSettingsCat == null && AdvancedRoleSettingsBanner.Count > 0)
            {
                RoleSettingsCat = Cats[1];
            }

            SetColorForCat(RoleAmountCat, Color.green);
            SetColorForCat(RoleSettingsCat, Color.blue);

            SetColorForRolesBanner(EngineerBanner, Utils.GetRoleColor(RoleTypes.Engineer), Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForRolesBanner(GuardianAngelBanner, Utils.GetRoleColor(RoleTypes.GuardianAngel), Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForRolesBanner(ScientistBanner, Utils.GetRoleColor(RoleTypes.Scientist), Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForRolesBanner(TrackerBanner, Utils.GetRoleColor(RoleTypes.Tracker), Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForRolesBanner(NoiseMakerBanner, Utils.GetRoleColor(RoleTypes.Noisemaker), Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForRolesBanner(ShapeShifterBanner, Utils.GetRoleColor(RoleTypes.Shapeshifter), Utils.GetRoleColor(RoleTypes.Impostor));
            SetColorForRolesBanner(PhantomBanner, Utils.GetRoleColor(RoleTypes.Phantom), Utils.GetRoleColor(RoleTypes.Impostor));

            SetColorForIcon(EngineerIcon, Utils.GetRoleColor(RoleTypes.Engineer), Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForIcon(GuardianAngelIcon, Utils.GetRoleColor(RoleTypes.GuardianAngel), Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForIcon(ScientistIcon, Utils.GetRoleColor(RoleTypes.Scientist), Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForIcon(TrackerIcon, Utils.GetRoleColor(RoleTypes.Tracker), Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForIcon(NoiseMakerIcon, Utils.GetRoleColor(RoleTypes.Noisemaker), Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForIcon(ShapeShifterIcon, Utils.GetRoleColor(RoleTypes.Shapeshifter), Utils.GetRoleColor(RoleTypes.Impostor));
            SetColorForIcon(PhantomIcon, Utils.GetRoleColor(RoleTypes.Phantom), Utils.GetRoleColor(RoleTypes.Impostor));
        }
        #endregion
        

    }
    public static void SetColorForRolesBanner(GameObject obj, Color iconcolor, Color bgcolor)
    {
        if (obj == null) return;
        if (obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color == new Color(0.3f, 0.3f, 0.3f, 1)) return;
        obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = bgcolor.ShadeColor(0.32f);
        obj.transform.FindChild("RoleIcon").gameObject.GetComponent<SpriteRenderer>().color = iconcolor;
    }

    public static void SetColorForIcon(GameObject obj, Color iconcolor, Color bgcolor)
    {
        if (obj == null) return;
        var cat = obj.transform.FindChild("CategoryHeaderRoleVariant");
        cat.FindChild("LabelSprite").gameObject.GetComponent<SpriteRenderer>().color = bgcolor.ShadeColor(0.32f);
        cat.FindChild("Divider").gameObject.GetComponent<SpriteRenderer>().color = bgcolor.ShadeColor(0.32f);
        cat.FindChild("HeaderText").gameObject.GetComponent<TextMeshPro>().color = Color.white;
        cat.FindChild("Icon").gameObject.GetComponent<SpriteRenderer>().color = iconcolor;
        obj.ForEachChild((Il2CppSystem.Action<GameObject>)SetColor);
        void SetColor(GameObject obj)
        {
            if (obj.name == "ViewSettingsInfoPanel(Clone)")
            {
                obj.transform.FindChild("Value").FindChild("Sprite").gameObject.GetComponent<SpriteRenderer>().color = iconcolor;
                obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = bgcolor.ShadeColor(0.38f);

            }
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
    public static void SetColorForSettingsOpt_StringAndNumber(GameObject obj, Color color)
    {
        obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.38f);
        obj.transform.FindChild("ValueBox").gameObject.GetComponent<SpriteRenderer>().color = color;
    }
    public static void SetColorForSettingsOpt_Checkbox(GameObject obj, Color color)
    {
        obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.38f);
        obj.transform.FindChild("Toggle").FindChild("InactiveSprite").gameObject.GetComponent<SpriteRenderer>().color = color;
    }

}

[HarmonyPatch]
class HnSLobbyViewSettingsPanePatch
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

    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Update)), HarmonyPostfix]
    static void Update()
    {
        if (XtremeGameData.GameStates.IsNormalGame) return;

        var Area = GameObject.Find("MainArea").transform.FindChild("Scaler").FindChild("Scroller").FindChild("SliderInner");
        Transform[] Banners = Area.GetComponentsInChildren<Transform>(true);

        #region 游戏设置
            List<GameObject> Cats = new();

            foreach (var Banner in Banners)
            {
                if (Banner.name == "CategoryHeaderMasked LongDivider(Clone)")
                {
                    Cats.Add(Banner.gameObject);
                }
            }

            if (Cats.Count < 4) return;
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


            List<GameObject> ViewSettingsInfoPanel = new();

            foreach (var Banner in Banners)
            {
                if (Banner.name == "ViewSettingsInfoPanel(Clone)")
                {
                    ViewSettingsInfoPanel.Add(Banner.gameObject);
                }
            }

            if (CrewmateSettingBanner1 == null)
            {
                CrewmateSettingBanner1 = ViewSettingsInfoPanel[0];
            }
            if (CrewmateSettingBanner2 == null)
            {
                CrewmateSettingBanner2 = ViewSettingsInfoPanel[1];
            }
            if (CrewmateSettingBanner3 == null)
            {
                CrewmateSettingBanner3 = ViewSettingsInfoPanel[2];
            }
            if (CrewmateSettingBanner4 == null)
            {
                CrewmateSettingBanner4 = ViewSettingsInfoPanel[3];
            }
            if (CrewmateSettingBanner5 == null)
            {
                CrewmateSettingBanner5 = ViewSettingsInfoPanel[4];
            }
            if (CrewmateSettingBanner6 == null)
            {
                CrewmateSettingBanner6 = ViewSettingsInfoPanel[5];
            }
            if (CrewmateSettingBanner7 == null)
            {
                CrewmateSettingBanner7 = ViewSettingsInfoPanel[6];
            }
            if (CrewmateSettingBanner8 == null)
            {
                CrewmateSettingBanner8 = ViewSettingsInfoPanel[7];
            }

            if (ImpostorSettingBanner1 == null)
            {
                ImpostorSettingBanner1 = ViewSettingsInfoPanel[8];
            }
            if (ImpostorSettingBanner2 == null)
            {
                ImpostorSettingBanner2 = ViewSettingsInfoPanel[9];
            }
            if (ImpostorSettingBanner3 == null)
            {
                ImpostorSettingBanner3 = ViewSettingsInfoPanel[10];
            }
            if (LastHiddenSettingBanner1 == null)
            {
                LastHiddenSettingBanner1 = ViewSettingsInfoPanel[11];
            }
            if (LastHiddenSettingBanner2 == null)
            {
                LastHiddenSettingBanner2 = ViewSettingsInfoPanel[12];
            }
            if (LastHiddenSettingBanner3 == null)
            {
                LastHiddenSettingBanner3 = ViewSettingsInfoPanel[13];
            }
            if (LastHiddenSettingBanner4 == null)
            {
                LastHiddenSettingBanner4 = ViewSettingsInfoPanel[14];
            }
            if (LastHiddenSettingBanner5 == null)
            {
                LastHiddenSettingBanner5 = ViewSettingsInfoPanel[15];
            }
            if (TaskSettingBanner1 == null)
            {
                TaskSettingBanner1 = ViewSettingsInfoPanel[16];
            }
            if (TaskSettingBanner2 == null)
            {
                TaskSettingBanner2 = ViewSettingsInfoPanel[17];
            }
            if (TaskSettingBanner3 == null)
            {
                TaskSettingBanner3 = ViewSettingsInfoPanel[18];
            }
            SetColorForCat(CrewmateCat, Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForCat(ImpostorCat, Utils.GetRoleColor(RoleTypes.Impostor));
            SetColorForCat(LastHiddenCat, Palette.Purple);
            SetColorForCat(TaskCat, Color.green);

            SetColorForSettingsBanner(CrewmateSettingBanner1, Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForSettingsBanner(CrewmateSettingBanner2, Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForSettingsBanner(CrewmateSettingBanner3, Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForSettingsBanner(CrewmateSettingBanner4, Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForSettingsBanner(CrewmateSettingBanner5, Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForSettingsBanner(CrewmateSettingBanner6, Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForSettingsBanner(CrewmateSettingBanner7, Utils.GetRoleColor(RoleTypes.Crewmate));
            SetColorForSettingsBanner(CrewmateSettingBanner8, Utils.GetRoleColor(RoleTypes.Crewmate));

            SetColorForSettingsBanner(ImpostorSettingBanner1, Utils.GetRoleColor(RoleTypes.Impostor));
            SetColorForSettingsBanner(ImpostorSettingBanner2, Utils.GetRoleColor(RoleTypes.Impostor));
            SetColorForSettingsBanner(ImpostorSettingBanner3, Utils.GetRoleColor(RoleTypes.Impostor));

            SetColorForSettingsBanner(LastHiddenSettingBanner1, Palette.Purple);
            SetColorForSettingsBanner(LastHiddenSettingBanner2, Palette.Purple);
            SetColorForSettingsBanner(LastHiddenSettingBanner3, Palette.Purple);
            SetColorForSettingsBanner(LastHiddenSettingBanner4, Palette.Purple);
            SetColorForSettingsBanner(LastHiddenSettingBanner5, Palette.Purple);

            SetColorForSettingsBanner(TaskSettingBanner1, Color.green);
            SetColorForSettingsBanner(TaskSettingBanner2, Color.green);
            SetColorForSettingsBanner(TaskSettingBanner3, Color.green);



        #endregion


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
    public static void SetColorForSettingsOpt_StringAndNumber(GameObject obj, Color color)
    {
        obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.38f);
        obj.transform.FindChild("ValueBox").gameObject.GetComponent<SpriteRenderer>().color = color;
    }
    public static void SetColorForSettingsOpt_Checkbox(GameObject obj, Color color)
    {
        obj.transform.FindChild("LabelBackground").gameObject.GetComponent<SpriteRenderer>().color = color.ShadeColor(0.38f);
        obj.transform.FindChild("Toggle").FindChild("InactiveSprite").gameObject.GetComponent<SpriteRenderer>().color = color;
    }

}
