using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;

namespace FinalSuspect_Xtreme.Patches;
[HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.UpdateValuesAndText))]
class RoleOptionSettingUpdateValuesAndTextPatch
{
    public static void Postfix(RoleOptionSetting __instance)
    {
        var rolecolor = Utils.GetRoleColor(__instance.Role.Role);
        __instance.labelSprite.color = Utils.ShadeColor(rolecolor, 0.2f);
        __instance.titleText.color = Color.white;
    }

}
[HarmonyPatch(typeof(GameOptionsManager), nameof(GameOptionsManager.SwitchGameMode))]
class SwitchGameModePatch
{
    public static bool HnSMode = false;
    public static void Postfix(GameModes gameMode)
    {
        if (gameMode != GameModes.HideNSeek) HnSMode = false;
        else HnSMode = true;


    }
}