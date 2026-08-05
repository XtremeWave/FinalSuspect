namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(HatManager))]
public static class HatManagerPatch
{
    [HarmonyPatch(nameof(HatManager.CheckLongModeValidCosmetic)),HarmonyPrefix]
    public static bool CheckLongMode_Prefix(out bool __result, ref string cosmeticID)
    {
        if (AprilFoolsMode.ShouldHorseAround())
        {
            __result = true;
            return false;
        }

        var flag = AprilFoolsMode.ShouldLongAround();

        if (flag && string.Equals("skin_rhm", cosmeticID))
        {
            __result = false;
            return false;
        }

        __result = true;
        return false;
    }
}