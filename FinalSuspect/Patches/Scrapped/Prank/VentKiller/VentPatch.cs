/*using HarmonyLib;

namespace FinalSuspect.Patches.Prank;

[HarmonyPatch(typeof(Vent))]
internal static class VentPatch
{
    // Token: 0x0600000B RID: 11 RVA: 0x000021F0 File Offset: 0x000003F0
    [HarmonyPatch(nameof(Vent.SetOutline))]
    [HarmonyPrefix]
    private static bool OutlinePatch(Vent __instance)
    {
        return !__instance.IsPlayer();
    }
}*/

