using HarmonyLib;

namespace FinalSuspect_Xtreme;

[HarmonyPatch]
public class MapRealTimeLocationPatch
{
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap)), HarmonyPostfix]
    public static void ShowNormalMapAfter(MapBehaviour __instance)
    {
        var roleType = PlayerControl.LocalPlayer.Data.Role.Role;
        var color = Utils.GetRoleColor(roleType);
        if (Main.EnableMapBackGround.Value)
            __instance.ColorControl.SetColor(color);
    }
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap)), HarmonyPostfix]
    public static void ShowSabotageMapAfter(MapBehaviour __instance)
    {
        var color = Palette.DisabledGrey;
        if (Main.EnableMapBackGround.Value)
            __instance.ColorControl.SetColor(color);
    }
}