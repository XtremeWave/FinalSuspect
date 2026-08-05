using System;

namespace FinalSuspect.Patches.System;


[HarmonyPatch(typeof(LongBoiPlayerBody))]
public static class LongBoiPlayerBodyPatches
{
    [HarmonyPatch(nameof(LongBoiPlayerBody.Awake))]
    [HarmonyPrefix]
    public static bool Awake_Prefix(LongBoiPlayerBody __instance)
    {
        //Fixes base-game layer issues
        __instance.cosmeticLayer.OnSetBodyAsGhost += (Action)__instance.SetPoolableGhost;
        __instance.cosmeticLayer.OnColorChange += (Action<int>)__instance.SetHeightFromColor;
        __instance.cosmeticLayer.OnCosmeticSet +=
            (Action<string, int, CosmeticsLayer.CosmeticKind>)__instance.OnCosmeticSet;
        __instance.gameObject.layer = 8;
        return false;
    }

    [HarmonyPatch(nameof(LongBoiPlayerBody.Start))]
    [HarmonyPrefix]
    public static bool Start_Prefix(LongBoiPlayerBody __instance)
    {
        //Fixes more runtime issues
        __instance.ShouldLongAround = true;
        if (__instance.hideCosmeticsQC) __instance.cosmeticLayer.SetHatVisorVisible(false);

        __instance.SetupNeckGrowth();
        if (__instance.isExiledPlayer)
        {
            var instance = ShipStatus.Instance;
            if (instance == null || instance.Type != ShipStatus.MapType.Fungle)
                __instance.cosmeticLayer.AdjustCosmeticRotations(-17.75f);
        }

        if (!__instance.isPoolablePlayer) __instance.cosmeticLayer.ValidateCosmetics();

        return false;
    }

    // Fix System.IndexOutOfRangeException: Index was outside the bounds of the array
    // When colorIndex is 255 them heightsPerColor[255] gets exception
    [HarmonyPatch(nameof(LongBoiPlayerBody.SetHeightFromColor))]
    [HarmonyPrefix]
    public static bool SetHeightFromColor_Prefix(int colorIndex)
    {
        return colorIndex != byte.MaxValue;
    }

    [HarmonyPatch(nameof(LongBoiPlayerBody.SetHeighFromDistanceHnS))]
    [HarmonyPrefix]
    public static bool SetHeighFromDistanceHnS_Prefix(LongBoiPlayerBody __instance, ref float distance)
    {
        //Remove the limit of neck size to prevent issues in TOHE HnS

        __instance.targetHeight = distance / 10f + 0.5f;
        __instance.SetupNeckGrowth(true); //se quiser sim mano
        return false;
    }

    
}
