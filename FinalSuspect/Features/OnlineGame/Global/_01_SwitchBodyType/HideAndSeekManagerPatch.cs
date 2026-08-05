namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(HideAndSeekManager))]
public static class HideAndSeekManagerPatch
{
    [HarmonyPatch(nameof(HideAndSeekManager.GetBodyType)), HarmonyPostfix]
    public static void GetBodyType_Postfix(ref PlayerBodyTypes __result, [HarmonyArgument(0)] PlayerControl player)
    {
        if (player == null || player.Data == null || player.Data.Role == null)
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
                    return;
            }

        switch (ConfigManager.SwitchOutfitType.Value)
        {
            case OutfitType.HorseMode when player.Data.Role.IsImpostor:
                __result = PlayerBodyTypes.Normal;
                return;
            case OutfitType.HorseMode:
                __result = PlayerBodyTypes.Horse;
                return;
            case OutfitType.LongMode when player.Data.Role.IsImpostor:
                __result = PlayerBodyTypes.LongSeeker;
                return;
            case OutfitType.LongMode:
                __result = PlayerBodyTypes.Long;
                return;
            case OutfitType.BeanMode:
            default:
            {
                if (player.Data.Role.IsImpostor)
                {
                    __result = PlayerBodyTypes.Seeker;
                    return;
                }

                __result = PlayerBodyTypes.Normal;
                return;
            }
        }
    }
}