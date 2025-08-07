using FinalSuspect.Modules.Core.Game;
using UnityEngine;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(RegionMenu))]
public static class RegionMenuPatch
{
    private static Scroller _scroller;

    [HarmonyPatch(nameof(RegionMenu.Awake))]
    [HarmonyPostfix]
    public static void Awake_Postfix(RegionMenu __instance)
    {
        if (_scroller) return;

        var back = __instance.ButtonPool.transform.FindChild("Backdrop");
        back.transform.localScale *= 10f;

        _scroller = __instance.ButtonPool.transform.parent.gameObject.AddComponent<Scroller>();
        _scroller.Inner = __instance.ButtonPool.transform;
        _scroller.MouseMustBeOverToScroll = true;
        _scroller.ClickMask = back.GetComponent<BoxCollider2D>();
        _scroller.ScrollWheelSpeed = 0.7f;
        _scroller.SetYBoundsMin(0f);
        _scroller.SetYBoundsMax(4f);
        _scroller.allowY = true;
    }

    [HarmonyPatch(nameof(RegionMenu.ChooseOption))]
    [HarmonyPostfix]
    public static void ChooseOption_Postfix()
    {
        ServerAddManager.SetServerName();
    }
}