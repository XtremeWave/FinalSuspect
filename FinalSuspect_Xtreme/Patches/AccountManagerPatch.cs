using HarmonyLib;
using TMPro;
using UnityEngine;

using static FinalSuspect_Xtreme.Translator;

namespace FinalSuspect_Xtreme;

[HarmonyPatch(typeof(AccountTab), nameof(AccountTab.Awake))]
public static class AwakeFriendCodeUIPatch
{
    private static GameObject BarSprit;
    private static GameObject CustomBarSprit;
    public static GameObject FriendsButton;
    public static void Prefix(AccountTab __instance)
    {


        if (BarSprit = GameObject.Find("BarSprite"))
        {
            CustomBarSprit = new();
            CustomBarSprit.transform.SetParent(BarSprit.transform.parent);
            CustomBarSprit.transform.localScale = BarSprit.transform.localScale;
            CustomBarSprit.transform.localPosition = BarSprit.transform.localPosition;

            static void ResetParent(GameObject obj)
            {
                obj.transform.SetParent(CustomBarSprit.transform);
            }
            BarSprit.ForEachChild((Il2CppSystem.Action<GameObject>)ResetParent);

            BarSprit.SetActive(false);

        }

        var newRequest = GameObject.Find("NewRequest");
        if (newRequest != null)
        {
            newRequest.transform.localPosition -= new Vector3(0f, 0f, 10f);
            newRequest.transform.localScale = new Vector3(0.8f, 1f, 1f);
        }


        FriendsButton = GameObject.Find("FriendsButton");
        if (FriendsButton != null)
        {
            //FriendsButton.SetActive(__instance.friendCode.gameObject.active);
        }

    }
}
[HarmonyPatch(typeof(AccountTab), nameof(AccountTab.UpdateVisuals))]
public static class UpdateFriendCodeUIPatch
{
    public static void Prefix(AccountTab __instance)
    {

    }
}