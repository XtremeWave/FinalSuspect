namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(NormalGameManager))]
public static class NormalGameManagerPatch
{
    
    [HarmonyPatch(nameof(NormalGameManager.GetBodyType)), HarmonyPostfix]
    public static void GetBodyType_Postfix(ref PlayerBodyTypes __result)
    {
        switch (ConfigManager.SwitchOutfitType.Value)
        {
            case OutfitType.HorseMode:
                __result = PlayerBodyTypes.Horse;
                return;
            case OutfitType.LongMode:
                __result = PlayerBodyTypes.Long;
                return;
            case OutfitType.BeanMode:
            default:
                __result = PlayerBodyTypes.Normal;
                break;
        }
    }
}