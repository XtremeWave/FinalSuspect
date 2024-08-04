using HarmonyLib;

namespace FinalSuspect_Xtreme.Patches;

[HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.SetFilterText))]
public static class HauntMenuMinigameSetFilterTextPatch
{
    public static bool Prefix(HauntMenuMinigame __instance)
    {
        if (__instance.HauntTarget != null)
        {
            // 役職表示をカスタムロール名で上書き
            __instance.NameText.text = Utils.ColorString(
                Utils.GetRoleColor(__instance.HauntTarget.GetRoleType()),
                __instance.NameText.text);
            __instance.FilterText.text = Utils.ColorString(
                Utils.GetRoleColor(__instance.HauntTarget.GetRoleType()),
                Utils.GetRoleName(__instance.HauntTarget.GetRoleType()));
            return false;
        }
        return true;
    }
}