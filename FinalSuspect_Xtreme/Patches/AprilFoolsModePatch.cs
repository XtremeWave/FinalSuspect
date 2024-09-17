using HarmonyLib;
using Il2CppSystem;
using InnerNet;
using static CosmeticsLayer;

namespace FinalSuspect_Xtreme;

// 来源：https://github.com/ykundesu/SuperNewRoles/blob/master/SuperNewRoles/Patches/HorseModePatch.cs
// 来源：TOHE
[HarmonyPatch(typeof(AprilFoolsMode), nameof(AprilFoolsMode.ShouldLongAround))]
public static class AprilFoolsModePatch
{
    public static bool Prefix(ref bool __result)
    {
        __result = Main.AprilFoolsMode.Value == Main.allAprilFoolsModes[2];
        return false;
    }
}


#region GameManager Patches
[HarmonyPatch(typeof(NormalGameManager), nameof(NormalGameManager.GetBodyType))]
public static class GetNormalBodyType_Patch
{
    public static void Postfix(ref PlayerBodyTypes __result)
    {
        if (Main.AprilFoolsMode.Value == Main.allAprilFoolsModes[1])
        {
            __result = PlayerBodyTypes.Horse;
            return;
        }
        if (AprilFoolsMode.ShouldLongAround())
        {
            __result = PlayerBodyTypes.Long;
            return;
        }
        __result = PlayerBodyTypes.Normal;
    }
}

[HarmonyPatch(typeof(HideAndSeekManager), nameof(HideAndSeekManager.GetBodyType))]
public static class GetHnsBodyType_Patch
{
    public static void Postfix(ref PlayerBodyTypes __result, [HarmonyArgument(0)] PlayerControl player)
    {
        if (player == null || player.Data == null || player.Data.Role == null)
        {
            if (Main.AprilFoolsMode.Value == Main.allAprilFoolsModes[1])
            {
                __result = PlayerBodyTypes.Horse;
                return;
            }
            if (AprilFoolsMode.ShouldLongAround())
            {
                __result = PlayerBodyTypes.Long;
                return;
            }
            __result = PlayerBodyTypes.Normal;
            return;
        }
        else if (Main.AprilFoolsMode.Value == Main.allAprilFoolsModes[1])
        {
            if (player.IsImpostor() && XtremeGameData.GameStates.IsInGame)
            {
                __result = PlayerBodyTypes.Normal;
                return;
            }
            __result = PlayerBodyTypes.Horse;
            return;
        }
        else if (AprilFoolsMode.ShouldLongAround())
        {
            if (player.IsImpostor() && XtremeGameData.GameStates.IsInGame)
            {
                __result = PlayerBodyTypes.LongSeeker;
                return;
            }
            __result = PlayerBodyTypes.Long;
            return;
        }
        else
        {
            if (player.IsImpostor() && XtremeGameData.GameStates.IsInGame)
            {
                __result = PlayerBodyTypes.Seeker;
                return;
            }
            __result = PlayerBodyTypes.Normal;
            return;
        }
    }
}
#endregion

#region LongBoi Patches
[HarmonyPatch(typeof(LongBoiPlayerBody))]
public static class LongBoiPatches
{
    [HarmonyPatch(nameof(LongBoiPlayerBody.Awake))]
    [HarmonyPrefix]
    public static bool LongBoyAwake_Patch(LongBoiPlayerBody __instance)
    {
        //Fixes base-game layer issues
        __instance.cosmeticLayer.OnSetBodyAsGhost += (Action)__instance.SetPoolableGhost;
        __instance.cosmeticLayer.OnColorChange += (Action<int>)__instance.SetHeightFromColor;
        __instance.cosmeticLayer.OnCosmeticSet += (Action<string, int, CosmeticKind>)__instance.OnCosmeticSet;
        __instance.gameObject.layer = 8;
        return false;
    }

    [HarmonyPatch(nameof(LongBoiPlayerBody.Start))]
    [HarmonyPrefix]
    public static bool LongBoyStart_Patch(LongBoiPlayerBody __instance)
    {
        //Fixes more runtime issues
        __instance.ShouldLongAround = true;
        if (__instance.hideCosmeticsQC)
        {
            __instance.cosmeticLayer.SetHatVisorVisible(false);
        }
        __instance.SetupNeckGrowth(false, true);
        if (__instance.isExiledPlayer)
        {
            ShipStatus instance = ShipStatus.Instance;
            if (instance == null || instance.Type != ShipStatus.MapType.Fungle)
            {
                __instance.cosmeticLayer.AdjustCosmeticRotations(-17.75f);
            }
        }
        if (!__instance.isPoolablePlayer)
        {
            __instance.cosmeticLayer.ValidateCosmetics();
        }
        return false;
    }

    [HarmonyPatch(nameof(LongBoiPlayerBody.SetHeighFromDistanceHnS))]
    [HarmonyPrefix]
    public static bool LongBoyNeckSize_Patch(LongBoiPlayerBody __instance, ref float distance)
    {
        //Remove the limit of neck size to prevent issues in FinalSuspect_XtremeE HnS

        __instance.targetHeight = distance / 10f + 0.5f;
        __instance.SetupNeckGrowth(true, true); //se quiser sim mano
        return false;
    }

    [HarmonyPatch(typeof(HatManager), nameof(HatManager.CheckLongModeValidCosmetic))]
    [HarmonyPrefix]
    public static bool CheckLongMode_Patch(out bool __result, ref string cosmeticID)
    {
        if (Main.AprilFoolsMode.Value == Main.allAprilFoolsModes[1])
        {
            __result = false;
            return false;
        }

        bool flag = AprilFoolsMode.ShouldLongAround();

        if (flag && string.Equals("skin_rhm", cosmeticID))
        {
            __result = false;
            return false;
        }
        __result = true;
        return false;
    }
}
#endregion

