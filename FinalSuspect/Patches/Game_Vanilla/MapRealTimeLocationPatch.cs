using FinalSuspect.DataHandling;
using HarmonyLib;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch]
public class MapRealTimeLocationPatch
{
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap)), HarmonyPostfix]
    public static void ShowNormalMapAfter(MapBehaviour __instance)
    {
        XtremeLocalHandling.ShowMap(__instance, true);
    }
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap)), HarmonyPostfix]
    public static void ShowSabotageMapAfter(MapBehaviour __instance)
    {
        XtremeLocalHandling.ShowMap(__instance, false);
    }
}