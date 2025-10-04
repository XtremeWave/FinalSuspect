using FinalSuspect.Modules.Core.Game.UI;
using static FinalSuspect.Modules.Core.Plugin.UI.MainMenu;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(AccountTab), nameof(AccountTab.Awake))]
public static class AwakeFriendCodeUIPatch
{
    public static void Prefix()
    {
        var barSprit = GameObject.Find("BarSprite");
        if (barSprit)
        {
            barSprit.GetComponent<SpriteRenderer>().color = Color.clear;
        }

        if (FriendsButton != null) return;
        FriendsButton = GameObject.Find("FriendsButton");
        FriendsButton.transform.FindChild("Highlight").FindChild("NewRequestActive").FindChild("Background").gameObject
            .GetComponent<SpriteRenderer>().color = Color.white.AlphaMultiplied(0.3f);
        FriendsButton.transform.FindChild("Inactive").FindChild("NewRequestInactive").FindChild("Background").gameObject
            .GetComponent<SpriteRenderer>().color = Color.white.AlphaMultiplied(0.3f);
    }
}

[HarmonyPatch(typeof(AccountManager), nameof(AccountManager.Awake))]
public static class AwakeAccountManager
{
    public static void Prefix(AccountManager __instance)
    {
        LoadingAnima.Create(__instance);
    }
}