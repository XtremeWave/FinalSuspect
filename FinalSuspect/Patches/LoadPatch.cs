using BepInEx.Unity.IL2CPP.Utils;
using static FinalSuspect.Modules.Managers.ResourcesManager.ResourcesDownloader;
using static FinalSuspect.Translator;
using System;
using HarmonyLib;
using System.Collections;
using UnityEngine;
using ListStr = System.Collections.Generic.List<string>;
using DictionaryStr = System.Collections.Generic.Dictionary<string, string>;
using System.IO;
using System.Collections.Generic;
using FinalSuspect.Modules.Managers.ResourcesManager;

namespace FinalSuspect.Patches;


public class LoadPatch
{
    //参考：TORCE
    static Sprite Team_Logo = Utils.LoadSprite("TeamLogo.png", 120f);
    static Sprite Glow = Utils.LoadSprite("FinalSuspect-Logo.png");
    static Sprite Mod_Logo = Utils.LoadSprite("FinalSuspect-Logo.png", 150f);
    static Sprite Mod_Logo_Blurred = Utils.LoadSprite("FinalSuspect-Logo.png", 150f);
    static TMPro.TextMeshPro loadText = null!;
    static TMPro.TextMeshPro loadText2 = null!;
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
            #region Anima
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
            #endregion
            loadText = GameObject.Instantiate(__instance.errorPopup.InfoText, null);
            loadText.transform.localPosition = new(0f, -0.28f, -10f);
            loadText.fontStyle = TMPro.FontStyles.Bold;
            loadText2 = GameObject.Instantiate(__instance.errorPopup.InfoText, null);
            loadText2.transform.localPosition = new(0f, -0.7f, -10f);
            loadText2.fontStyle = TMPro.FontStyles.Bold;
            loadText.color = Color.white.AlphaMultiplied(0.3f);
            loadText.text = GetString("Loading");

            #region LoadAmongUs
            loadText2.color = Color.white.AlphaMultiplied(0.3f);
            loadText2.text = "Loading Translation...";

            yield return DestroyableSingleton<ReferenceDataManager>.Instance.Initialize();
            try
            {
                DestroyableSingleton<TranslationController>.Instance.Initialize();
            }
            catch
            { }
            yield return new WaitForSeconds(0.5f);
            #endregion
            ListStr remoteDependList = new()
        {
            "YamlDotNet.dll",
            "YamlDotNet.xml",
        };
            if (!Directory.Exists(ImagesSavePath))
                Directory.CreateDirectory(ImagesSavePath);
            if (!Directory.Exists(DependsSavePath))
                Directory.CreateDirectory(DependsSavePath);
            string remoteResourcesUrl;

            remoteResourcesUrl = IsChineseLanguageUser ? DependsdownloadUrl_gitee : DependsdownloadUrl_github;

            foreach (var resource in remoteDependList)
            {
                string localFilePath = DependsSavePath + resource;
                if (!File.Exists(localFilePath))
                {
                    var task = StartDownload(remoteResourcesUrl + "Depends/" + resource, localFilePath);
                    while (!task.IsCompleted)
                    {
                        yield return null;
                    }

                    if (task.IsFaulted)
                    {
                        Logger.Error($"Download of {remoteResourcesUrl + "Depends/" + resource} failed: {task.Exception}", "Download Resource");
                    }
                }
            }

            loadText2.text = GetString("LanguageFilesLoadingComplete");
            yield return new WaitForSeconds(1f);
            #region Checking
            loadText2.text = GetString("CheckingForFiles");

            while (!VersionChecker.isChecked)
            {
                yield return null;
            }
            ListStr remoteImageList = new()
        {
            "CI_Crewmate.png",
            "CI_CrewmateGhost.png",
            "CI_Engineer.png",
            "CI_GuardianAngel.png",
            "CI_HnSEngineer.png",
            "CI_HnSImpostor.png",
            "CI_Impostor.png",
            "CI_ImpostorGhost.png",
            "CI_Noisemaker.png",
            "CI_Phantom.png",
            "CI_Scientist.png",
            "CI_Shapeshifter.png",
            "CI_Tracker.png",
            "DleksBanner.png",
            "DleksBanner-Wordart.png",
            "DleksButton.png",
            "FinalSuspect-BG.jpg",
            "RightPanelCloseButton.png",
        };
            remoteResourcesUrl = IsChineseLanguageUser ? ImagedownloadUrl_gitee : ImagedownloadUrl_github;

            DictionaryStr needDownloadsPath = new();
            foreach (var resource in remoteImageList)
            {
                string localFilePath = ImagesSavePath + resource;
                if (!File.Exists(localFilePath))
                {
                    needDownloadsPath.Add(remoteResourcesUrl + "Images/" + resource, localFilePath);
                    Logger.Warn($"File do not exists: {localFilePath}", "Check");
                }
            }
            #endregion
            #region Downloading
            yield return new WaitForSeconds(0.5f);
            FileAttributes attributes = File.GetAttributes(ImagesSavePath);
            File.SetAttributes(ImagesSavePath, attributes | FileAttributes.Hidden);
            int process = 0;
            foreach (var resources in needDownloadsPath)
            {
                process++;
                Color yellow = new Color32(252, 255, 152, 255);
                loadText2.color = yellow.AlphaMultiplied(0.3f);

                loadText2.text = GetString("DownloadingResources") + $"({process}/{needDownloadsPath.Count})";
                var task = StartDownload(resources.Key, resources.Value);
                while (!task.IsCompleted)
                {
                    yield return null; 
                }

                if (task.IsFaulted)
                {
                    Logger.Error($"Download of {resources.Key} failed: {task.Exception}", "Download Resource");
                }
            }
            yield return new WaitForSeconds(0.5f);
            GameObject.Destroy(loadText2.gameObject);

            #endregion

            yield return new WaitForSeconds(1f);
            Color green = new Color32(185, 255, 181, 255);
            loadText.color = green.AlphaMultiplied(0.3f);

            loadText.text = GetString("LoadingComplete");
            for (int i = 0; i < 3; i++)
            {
                loadText.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.03f);
                loadText.gameObject.SetActive(true);
                yield return new WaitForSeconds(0.03f);
            }
            yield return new WaitForSeconds(0.5f);

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