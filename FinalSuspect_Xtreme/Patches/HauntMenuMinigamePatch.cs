using HarmonyLib;

namespace FinalSuspect_Xtreme.Patches;

[HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.SetFilterText))]
public static class HauntMenuMinigameSetFilterTextPatch
{
    public static bool Prefix(HauntMenuMinigame __instance)
    {
        if (__instance.HauntTarget != null)
        {
            var role = __instance.HauntTarget.GetRoleType();
            var color = Utils.GetRoleColor(role);
            var rn = Utils.GetRoleName(role);
            __instance.NameText.text = Utils.ColorString(color, __instance.NameText.text);
            __instance.FilterText.text = Utils.ColorString(color, rn);
            return false;
        }
        return true;
    }
}