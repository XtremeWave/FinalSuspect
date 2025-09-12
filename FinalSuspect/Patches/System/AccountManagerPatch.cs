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
        LoadSprite("CI_ImpostorGhost.png", 450f),
        LoadSprite("CI_Viper.png", 450f),
        LoadSprite("CI_Detective.png", 450f)
    ];

    private static int _currentIndex;

    private static GameObject _crewpetWalk0001;
    private static GameObject _modLoading;

    public static void Prefix(AccountManager __instance)
    {
        try
        {
            var loading = GameObject.Find("Loading");
            loading.SetActive(false);

            var bgf = GameObject.Find("BackgroundFill");
            _crewpetWalk0001 = bgf.transform.FindChild("crewpet_walk0001").gameObject;
            var r = _crewpetWalk0001.GetComponent<WaitingRotate>();
            r.speed = 0f;
            _modLoading = new GameObject("ModLoading");
            _modLoading.transform.SetParent(_crewpetWalk0001.transform.parent);
            _modLoading.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            _modLoading.transform.localPosition = new Vector3(4.5f, -2.4f, -1f);
            var sprite = _modLoading.AddComponent<SpriteRenderer>();
            sprite.color = Color.white;
            sprite.flipX = false;
            __instance.StartCoroutine(SwitchRoleIllustration(sprite));
            _crewpetWalk0001.SetActive(false);

            var ap = _modLoading.AddComponent<AspectPosition>();
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

            spriter.sprite = AllRoleRoleIllustration[_currentIndex];
            var p = 1f;
            while (p > 0f)
            {
                p -= Time.deltaTime * 2.8f;
                var alpha = 1 - p;
                spriter.color = Color.white.AlphaMultiplied(alpha);
                yield return null;
            }

            _currentIndex = (_currentIndex + 1) % AllRoleRoleIllustration.Length;

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