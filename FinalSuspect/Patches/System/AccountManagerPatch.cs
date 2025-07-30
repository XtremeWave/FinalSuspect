using System.Collections;
using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;
using static FinalSuspect.Modules.Core.Plugin.ModMainMenuManager;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(AccountTab), nameof(AccountTab.Awake))]
public static class AwakeFriendCodeUIPatch
{
    public static void Prefix()
    {
        var BarSprit = GameObject.Find("BarSprite");
        if (BarSprit)
        {
            BarSprit.GetComponent<SpriteRenderer>().color = Color.clear;
        }

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
    public static readonly Sprite[] AllRoleRoleIllustration =
    [
        LoadSprite("CI_Crewmate.png", 450f),
        LoadSprite("CI_HnSEngineer.png", 450f),
        LoadSprite("CI_Engineer.png", 450f),
        LoadSprite("CI_GuardianAngel.png", 450f),
        LoadSprite("CI_Scientist.png", 450f),
        LoadSprite("CI_Tracker.png", 450f),
        LoadSprite("CI_Noisemaker.png", 450f),
        LoadSprite("CI_CrewmateGhost.png", 450f),
        LoadSprite("CI_Impostor.png", 450f),
        LoadSprite("CI_HnSImpostor.png", 450f),
        LoadSprite("CI_Shapeshifter.png", 450f),
        LoadSprite("CI_Phantom.png", 450f),
        LoadSprite("CI_ImpostorGhost.png", 450f)
    ];

    private static int currentIndex;

    private static GameObject crewpet_walk0001;
    private static GameObject ModLoading;

    public static void Prefix(AccountManager __instance)
    {
        try
        {
            var loading = GameObject.Find("Loading");
            loading.SetActive(false);

            var bgf = GameObject.Find("BackgroundFill");
            crewpet_walk0001 = bgf.transform.FindChild("crewpet_walk0001").gameObject;
            var r = crewpet_walk0001.GetComponent<WaitingRotate>();
            r.speed = 0f;
            ModLoading = new GameObject("ModLoading");
            ModLoading.transform.SetParent(crewpet_walk0001.transform.parent);
            ModLoading.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            ModLoading.transform.localPosition = new Vector3(4.5f, -2.4f, -1f);
            var Sprite = ModLoading.AddComponent<SpriteRenderer>();
            Sprite.color = Color.white;
            Sprite.flipX = false;
            __instance.StartCoroutine(SwitchRoleIllustration(Sprite));
            crewpet_walk0001.SetActive(false);

            var ap = ModLoading.AddComponent<AspectPosition>();
            ap.Alignment = AspectPosition.EdgeAlignments.RightBottom;
            ap.DistanceFromEdge = new Vector3(0.6f, 0.5f, -1000);
            ap.updateAlways = true;
        }
        catch
        {
            /* ignored */
        }
    }

    private static IEnumerator SwitchRoleIllustration(SpriteRenderer spriter)
    {
        while (true)
        {
            if (AllRoleRoleIllustration.Length == 0) yield break;

            spriter.sprite = AllRoleRoleIllustration[currentIndex];
            var p = 1f;
            while (p > 0f)
            {
                p -= Time.deltaTime * 2.8f;
                var alpha = 1 - p;
                spriter.color = Color.white.AlphaMultiplied(alpha);
                yield return null;
            }

            currentIndex = (currentIndex + 1) % AllRoleRoleIllustration.Length;

            yield return new WaitForSeconds(1f);
            p = 1f;
            while (p > 0f)
            {
                p -= Time.deltaTime * 2.8f;
                spriter.color = Color.white.AlphaMultiplied(p);
                yield return null;
            }
        }
    }
}