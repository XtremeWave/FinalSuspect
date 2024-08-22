﻿using HarmonyLib;
using UnityEngine;
using System.Collections;
using UnityEngine.Animations;
using BepInEx.Unity.IL2CPP.Utils;

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

    }
}
[HarmonyPatch(typeof(AccountManager), nameof(AccountManager.Awake))]
class AwakeAccountManager
{
    public static Sprite[] sprites = { 
        Utils.LoadSprite("FinalSuspect_Xtreme.Resources.Images.CI_Crewmate.png", 450f),
        //Utils.LoadSprite("FinalSuspect_Xtreme.Resources.Images.CI_Scientist.png", 450f),
        Utils.LoadSprite("FinalSuspect_Xtreme.Resources.Images.CI_Engineer.png", 450f),
        //Utils.LoadSprite("FinalSuspect_Xtreme.Resources.Images.CI_Tracker.png", 450f),
        //Utils.LoadSprite("FinalSuspect_Xtreme.Resources.Images.CI_Noisemaker.png", 450f),
        //Utils.LoadSprite("FinalSuspect_Xtreme.Resources.Images.CI_CrewmateGhost.png", 450f),
        //Utils.LoadSprite("FinalSuspect_Xtreme.Resources.Images.CI_GuardainAngel.png", 450f),
        //Utils.LoadSprite("FinalSuspect_Xtreme.Resources.Images.CI_Impostor.png", 450f),
        //Utils.LoadSprite("FinalSuspect_Xtreme.Resources.Images.CI_Shapeshifter.png", 450f),
        //Utils.LoadSprite("FinalSuspect_Xtreme.Resources.Images.CI_Phantom.png", 450f),
        //Utils.LoadSprite("FinalSuspect_Xtreme.Resources.Images.CI_ImpostorGhost.png", 450f),


    }; 
    private static int currentIndex = 0;

    static GameObject crewpet_walk0001;
    static GameObject ModLoading;
    public static void Prefix(AccountManager __instance)
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
        ModLoading.transform.localPosition = new Vector3(4.5f, - 2.4f, - 1f);
        var Sprite = ModLoading.AddComponent<SpriteRenderer>();
        Sprite.color = Color.white;
        Sprite.flipX = false;
        __instance.StartCoroutine(SwitchCharacterIllustration(Sprite));
        crewpet_walk0001.SetActive(false);

        static IEnumerator SwitchCharacterIllustration(SpriteRenderer spriter)
        {
            while (true)
            {
                if (sprites.Length == 0) yield break;

                spriter.sprite = sprites[currentIndex];
                var p = 1f;
                while (p > 0f)
                {
                    p -= Time.deltaTime * 2.8f;
                    float alpha = 1 - p;
                    spriter.color = Color.white.AlphaMultiplied(alpha);
                    yield return null;
                }
                currentIndex = (currentIndex + 1) % sprites.Length;


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
}
