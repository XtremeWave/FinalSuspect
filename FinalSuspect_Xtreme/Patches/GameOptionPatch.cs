using AmongUs.GameOptions;
using HarmonyLib;
using TMPro;
using UnityEngine;

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
        var header = GameObject.Find("HeaderButtons");

        if (AllButton == null)
        {
            AllButton = header.transform.FindChild("AllButton").gameObject;
        }
        AllButton.transform.FindChild("Highlight").gameObject.GetComponent<SpriteRenderer>().color = Main.ModColor32;
        AllButton.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().color = Main.ModColor32;
        AllButton.transform.FindChild("Selected").gameObject.GetComponent<SpriteRenderer>().color = Main.ModColor32;
        var color = AllButton.transform.FindChild("Text").gameObject.GetComponent<TextMeshPro>().color;
        if (color == Color.white || color == Main.ModColor32)
            AllButton.transform.FindChild("Text").gameObject.GetComponent<TextMeshPro>().color = Main.ModColor32;
        else
            AllButton.transform.FindChild("Text").gameObject.GetComponent<TextMeshPro>().color = new Color(0.45f, 0.45f, 0.65f);

        if (EngineerButton == null)
        {
            EngineerButton = header.transform.GetChild(4).gameObject;
        }
        SetColor(EngineerButton, Utils.GetRoleColor(RoleTypes.Engineer), Utils.GetRoleColor(RoleTypes.Crewmate));


        if (GuardianAngelButton == null)
        {
            GuardianAngelButton = header.transform.GetChild(5).gameObject;
        }
        SetColor(GuardianAngelButton, Utils.GetRoleColor(RoleTypes.GuardianAngel), Utils.GetRoleColor(RoleTypes.Crewmate));

        if (ScientistButton == null)
        {
            ScientistButton = header.transform.GetChild(6).gameObject;
        }
        SetColor(ScientistButton, Utils.GetRoleColor(RoleTypes.Scientist), Utils.GetRoleColor(RoleTypes.Crewmate));

        if (TrackerButton == null)
        {
            TrackerButton = header.transform.GetChild(7).gameObject;
        }
        SetColor(TrackerButton, Utils.GetRoleColor(RoleTypes.Tracker), Utils.GetRoleColor(RoleTypes.Crewmate));

        if (NoiseMakerButton == null)
        {
            NoiseMakerButton = header.transform.GetChild(8).gameObject;
        }
        SetColor(NoiseMakerButton, Utils.GetRoleColor(RoleTypes.Noisemaker), Utils.GetRoleColor(RoleTypes.Crewmate));

        if (ShapeShifterButton == null)
        {
            ShapeShifterButton = header.transform.GetChild(9).gameObject;
        }
        SetColor(ShapeShifterButton, Utils.GetRoleColor(RoleTypes.Shapeshifter), Utils.GetRoleColor(RoleTypes.Impostor));

        if (PhantomButton == null)
        {
            PhantomButton = header.transform.GetChild(10).gameObject;
        }
        SetColor(PhantomButton, Utils.GetRoleColor(RoleTypes.Phantom), Utils.GetRoleColor(RoleTypes.Impostor));

    }
    static void SetColor(GameObject obj, Color iconcolor, Color bgcolor)
    {
        obj.transform.FindChild("SelectedHighlight").gameObject.GetComponent<SpriteRenderer>().color = bgcolor;
        obj.transform.FindChild("HoverHighlight").gameObject.GetComponent<SpriteRenderer>().color = bgcolor; 
        obj.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().color = bgcolor;
        obj.transform.FindChild("RoleIcon").gameObject.GetComponent<SpriteRenderer>().color = iconcolor;
    }
}