using UnityEngine;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch]
public class MapRealTimeLocationPatch
{
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Show)), HarmonyPostfix]
    public static void ShowMapAfter(MapBehaviour __instance, [HarmonyArgument(0)] MapOptions opts)
    {
        XtremeLocalHandling.ShowMap(__instance, opts);
        
    }
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Awake)), HarmonyPostfix]
    public static void AwakeAfter(MapBehaviour __instance)
    {
        foreach (var player in Main.AllPlayerControls)
        {
            var rend = Object.Instantiate(__instance.HerePoint);
            rend.transform.SetParent(__instance.HerePoint.transform.parent);
            rend.gameObject.SetActive(false);
            player.GetXtremeData().rend = rend;

        }
    }
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate)), HarmonyPostfix]
    public static void FixedUpdateAfter(MapBehaviour __instance)
    {
        XtremeLocalHandling.UpdateMap();
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.SetPreMeetingPosition)), HarmonyPostfix]
    public static void SetPreMeetingPositionAfter(MapBehaviour __instance, [HarmonyArgument(0)] Vector3 preMeetingPosition)
    {
        foreach (var data in XtremePlayerData.AllPlayerData)
        {
            data.preMeetingPosition = data.Player.GetTruePosition();
        }
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.GenericShow)), HarmonyPostfix]
    public static void GenericShowAfter(MapBehaviour __instance)
    {
        foreach (var data in XtremePlayerData.AllPlayerData)
        {
            data.rend.material.SetInt(PlayerMaterial.MaskLayer, 255);
        }
        
    }
}