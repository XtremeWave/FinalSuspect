using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace FinalSuspect_Xtreme.Patches;


public class LoadPatch
{
    static Sprite Team_Logo = Utils.LoadSprite("FinalSuspect_Xtreme.Resources.Images.LobbyPaint.png", 120f);
    static Sprite Glow = Utils.LoadSprite("FinalSuspect_Xtreme.Resources.Images.FinalSuspect_Xtreme-Logo.png");
    static Sprite Mod_Logo = Utils.LoadSprite("FinalSuspect_Xtreme.Resources.Images.FinalSuspect_Xtreme-Logo.png", 150f);
    static Sprite Mod_Logo_Blurred = Utils.LoadSprite("FinalSuspect_Xtreme.Resources.Images.FinalSuspect_Xtreme-Logo-Blurred.png", 150f);
    static TMPro.TextMeshPro loadText = null!;
    public static string LoadingText { set { loadText.text = value; } }
    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Start))]
    class Start
    {
        static bool Prefix(SplashManager __instance)
        {
            ResolutionManager.SetResolution(1920, 1080, true);
            __instance.startTime = Time.time;
            __instance.StartCoroutine(InitializeRefdata(__instance));
            return false;
        }
        private static IEnumerator InitializeRefdata(SplashManager __instance)
        {
            var LogoAnimator = GameObject.Find("LogoAnimator");
            LogoAnimator.SetActive(false);
            yield return new WaitForSeconds(2f);

            var teamlogo = ObjectHelper.CreateObject<SpriteRenderer>("Team_Logo", null, new Vector3(0, 0f, -5f));
            teamlogo.sprite = Team_Logo;

            var p = 1f;
            while (p > 0f)
            {
                p -= Time.deltaTime * 2.8f;
                float alpha = 1 - p;
                teamlogo.color = Color.white.AlphaMultiplied(alpha);
                yield return null;
            }
            yield return new WaitForSeconds(1.5f);
            p = 1f;
            while (p > 0f)
            {
                p -= Time.deltaTime * 2.8f;
                teamlogo.color = Color.white.AlphaMultiplied(p);
                yield return null;
            }
            yield return new WaitForSeconds(2f);
            var modlogo = ObjectHelper.CreateObject<SpriteRenderer>("Mod_Logo", null, new Vector3(0, 0.3f, -5f));
            var modlogo_Blurred = ObjectHelper.CreateObject<SpriteRenderer>("Mod_Logo_Blurred", null, new Vector3(0, 0.3f, -5f));
            var glow = ObjectHelper.CreateObject<SpriteRenderer>("glow", null, new Vector3(0, 0.3f, -5f));
            modlogo.sprite = Mod_Logo;
            modlogo_Blurred.sprite = Mod_Logo_Blurred;
            glow.sprite = Glow;

            p = 1f;
            while (p > 0f)
            {
                p -= Time.deltaTime * 2.8f;
                float alpha = 1 - p;
                modlogo.color = Color.white.AlphaMultiplied(alpha);
                glow.color = Color.white.AlphaMultiplied(alpha);
                modlogo_Blurred.color = Color.white.AlphaMultiplied(Mathf.Min(1f, alpha * (p * 2)));
                modlogo.transform.localScale = Vector3.one * (p * p * 0.012f + 1f);
                modlogo_Blurred.transform.localScale = Vector3.one * (p * p * 0.012f + 1f);
                yield return null;
            }
            modlogo.color = Color.white;
            modlogo_Blurred.gameObject.SetActive(false);
            modlogo.transform.localScale = Vector3.one;

            loadText = GameObject.Instantiate(__instance.errorPopup.InfoText, null);
            loadText.transform.localPosition = new(0f, -0.28f, -10f);
            loadText.fontStyle = TMPro.FontStyles.Bold;
            loadText.text = "Loading...";
            loadText.color = Color.white.AlphaMultiplied(0.3f);
            yield return DestroyableSingleton<ReferenceDataManager>.Instance.Initialize();
            try
            {
                DestroyableSingleton<TranslationController>.Instance.Initialize();
            }
            catch
            { }
            yield return new WaitForSeconds(0.5f);

            loadText.text = "Loading Completed!";
            for (int i = 0; i < 3; i++)
            {
                loadText.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.03f);
                loadText.gameObject.SetActive(true);
                yield return new WaitForSeconds(0.03f);
            }

            GameObject.Destroy(loadText.gameObject);

            p = 1f;
            while (p > 0f)
            {
                glow.color = Color.white.AlphaMultiplied(p);
                p -= Time.deltaTime * 1.2f;
                modlogo.color = Color.white.AlphaMultiplied(p);
                yield return null;
            }
            modlogo.color = Color.clear;

            __instance.loadingObject.SetActive(false);
            __instance.sceneChanger.BeginLoadingScene();
            __instance.doneLoadingRefdata = true;
            yield break;
        }

    }
    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
    class Update
    {
        static bool Prefix(SplashManager __instance)
        {
            if (__instance.doneLoadingRefdata && !__instance.startedSceneLoad && Time.time - __instance.startTime > __instance.minimumSecondsBeforeSceneChange)
            {
                __instance.sceneChanger.AllowFinishLoadingScene();
                __instance.startedSceneLoad = true;
            }
            return false;
        }
    }



}