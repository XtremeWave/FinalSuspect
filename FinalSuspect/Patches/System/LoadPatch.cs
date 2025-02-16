using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx.Unity.IL2CPP.Utils;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Modules.Resources;
using TMPro;
using UnityEngine;
using static FinalSuspect.Modules.Resources.ResourcesDownloader;

namespace FinalSuspect.Patches.System;


public class LoadPatch
{
    private static List<string> preReadyRemoteImageList =
    [
        "FinalSuspect-Logo.png",
        "FinalSuspect-Logo-Blurred.png",
        "LastResult-BG.png",
        "TeamLogo.png"
    ];

    private static List<string> remoteImageList =
    [
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
        "Cursor.png",
        "DleksBanner.png",
        "DleksBanner-Wordart.png",
        "DleksButton.png",
        "FinalSuspect-BG-MiraHQ.jpg",
        "FinalSuspect-BG-NewYear.png",
        "ModStamp.png",
        "RightPanelCloseButton.png"
    ];

    private static List<string> remoteDependList =
    [
        "YamlDotNet.dll",
        "YamlDotNet.xml"
    ];

    private static List<string> remoteModNewsList =
    [
        "FS.v1.0_20250129.txt",
        "FeaturesIntroduction.v1.0.txt",
        "FS.v1.0_20250216.txt",
    ];

    private static List<string> remoteLanguageList =
    [
        "Brazilian.yaml",
        "SChinese.yaml",
        "TChinese.yaml",
        "Dutch.yaml",
        "English.yaml",
        "Filipino.yaml",
        "Franch.yaml",
        "German.yaml",
        "Irish.yaml",
        "Italian.yaml",
        "Japanese.yaml",
        "Korean.yaml",
        "Latam.yaml",
        "Portuguese.yaml",
        "Russian.yaml",
        "Spanish.yaml"
    ];

    private static TextMeshPro LoadText = null!;
    private static TextMeshPro ProcessText = null!;
    private static SpriteRenderer Teamlogo = null!;
    private static SpriteRenderer Modlogo = null!;
    private static SpriteRenderer ModlogoBlurred = null!;
    private static SpriteRenderer Glow = null!;
    private static bool ReloadLanguage;
    private static bool SkipLoadAnima = false;

    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Start))]
    private class Start
    {
        private static bool Prefix(SplashManager __instance)
        {
            ResolutionManager.SetResolution(1920, 1080, Screen.fullScreen);
            __instance.startTime = Time.time;
            __instance.StartCoroutine(InitializeRefdata(__instance));
            return false;
        }

        private static IEnumerator InitializeRefdata(SplashManager __instance)
        {
            #region Resources and variables
            
            LoadText = GameObject.Instantiate(__instance.errorPopup.InfoText, null);
            LoadText.transform.localPosition = new(0f, 0, -10f);
            LoadText.fontStyle = FontStyles.Bold;
            LoadText.text = null;
            
            ProcessText = GameObject.Instantiate(__instance.errorPopup.InfoText, null);
            ProcessText.transform.localPosition = new(0f, -0.7f, -10f);
            ProcessText.fontStyle = FontStyles.Bold;
            ProcessText.text = null;

            float p;
            
            var reloadBypassPath_Once = PathManager.GetBypassFileType(FileType.Languages, BypassType.Once);
            var reloadBypassPath_Longterm = PathManager.GetBypassFileType(FileType.Languages, BypassType.Longterm);
            var thisversion = $"{Main.PluginVersion}|{Main.DisplayedVersion}|{ThisAssembly.Git.Commit}-{ThisAssembly.Git.Branch}";
            var writeinVer = false;
            ReloadLanguage = thisversion != Main.LastStartVersion.Value 
                             && !(File.Exists(reloadBypassPath_Once) || File.Exists(reloadBypassPath_Longterm));

            if (File.Exists(reloadBypassPath_Once) || File.Exists(reloadBypassPath_Longterm))
            {
                if (File.Exists(reloadBypassPath_Once))
                {
                    File.Delete(reloadBypassPath_Once);
                }
            }
            else
            {
                writeinVer = true;
            }

            var fastboot = Main.FastBoot.Value && !ReloadLanguage;
            
            #endregion

            var logoAnimator = GameObject.Find("LogoAnimator");
            logoAnimator.SetActive(false);

            #region First Launch Final Suspect
            
            CheckForListResources_Remove(ref preReadyRemoteImageList, FileType.Images);
            yield return DownloadListResources(preReadyRemoteImageList, FileType.Images,
                () =>
                {
                    fastboot = false;
                    LoadText.text = $"Welcome to <color={ColorHelper.ModColor}>FinalSuspect</color>.";
                });

            if (!string.IsNullOrEmpty(LoadText.text))
            {
                LoadText.text = null;
                yield return new WaitForSeconds(2f);
            }
            
            #endregion

            LoadText.transform.localPosition = new(0f, -0.28f, -10f);
            LoadText.SetOutlineColor(Color.black);
            LoadText.SetOutlineThickness(0.15f);
            
            ProcessText.SetOutlineColor(Color.black);
            ProcessText.SetOutlineThickness(0.15f);
            
            Teamlogo = ObjectHelper.CreateObject<SpriteRenderer>("Team_Logo", null, new Vector3(0, 0f, -5f));
            Teamlogo.sprite = Utils.LoadSprite("TeamLogo.png", 120f);
            Teamlogo.color = Color.clear;
            
            Modlogo = ObjectHelper.CreateObject<SpriteRenderer>("Mod_Logo", null, new Vector3(0, 0.3f, -5f));
            Modlogo.sprite = Utils.LoadSprite("FinalSuspect-Logo.png", 150f);
            Modlogo.color = Color.clear;
            
            ModlogoBlurred = ObjectHelper.CreateObject<SpriteRenderer>("Mod_Logo_Blurred", null, new Vector3(0, 0.3f, -5f));
            ModlogoBlurred.sprite = Utils.LoadSprite("FinalSuspect-Logo-Blurred.png", 150f);
            ModlogoBlurred.color = Color.clear;
            
            Glow = ObjectHelper.CreateObject<SpriteRenderer>("Glow", null, new Vector3(0, 0.3f, -5f));
            Glow.sprite = Utils.LoadSprite("FinalSuspect-Logo.png");
            Glow.color = Color.clear;

            #region Fast Boot

            switch (fastboot)
            {
                case true:
                {
                    Teamlogo.color = Color.white;
                    Teamlogo.transform.localPosition = new Vector3(0, 1.7f, -5f);
                    Teamlogo.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                    Modlogo.color = Color.white;
                    Modlogo.transform.localPosition = new Vector3(0, 0, -5f);
                    Modlogo.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                    Glow.color = Color.green;
                    if (writeinVer) Main.LastStartVersion.Value = thisversion;
                    Init();
                    ProcessText.text = GetString("FastBoot");
                    ProcessText.color = Color.green;
                    ProcessText.transform.localPosition = new Vector3(0, -0.7f, -5f);
                    yield return new WaitForSeconds(1f);
                    SkipLoadAnima = true;
                    break;
                }
                case false:
                {
                    #region Team Logo Anima

                    yield return new WaitForSeconds(0.5f);
                    p = 1f;
                    while (p > 0f)
                    {
                        p -= Time.deltaTime * 2.8f;
                        var alpha = 1 - p;
                        Teamlogo.color = Color.white.AlphaMultiplied(alpha);
                        yield return null;
                    }
                
                    yield return new WaitForSeconds(1.5f);

                    p = 1f;
                    while (p > 0f)
                    {
                        p -= Time.deltaTime * 2.8f;
                        Teamlogo.color = Color.white.AlphaMultiplied(p);
                        yield return null;
                    }

                    yield return new WaitForSeconds(2f);

                    #endregion
            
                    #region Start Load

                    p = 1f;
                    while (p > 0f)
                    {
                        p -= Time.deltaTime * 2.8f;
                        var alpha = 1 - p;
                        if (fastboot)
                            Glow.color = Color.white.AlphaMultiplied(alpha);
                        Modlogo.color = Color.white.AlphaMultiplied(alpha);
                        ModlogoBlurred.color = Color.white.AlphaMultiplied(Mathf.Min(1f, alpha * (p * 2)));
                        Modlogo.transform.localScale = Vector3.one * (p * p * 0.012f + 1f);
                        ModlogoBlurred.transform.localScale = Vector3.one * (p * p * 0.012f + 1f);
                        yield return null;
                    }

                    Modlogo.color = Color.white;
                    ModlogoBlurred.gameObject.SetActive(false);
                    Modlogo.transform.localScale = Vector3.one;
                    if (!fastboot)
                        yield return new WaitForSeconds(0.75f);

                    if (!fastboot)
                    {
                        LoadText.color = Color.white.AlphaMultiplied(0.75f);
                        LoadText.text = "Loading...";
                        p = 1f;
                        while (p > 0)
                        {
                            p -= Time.deltaTime * 2.8f;
                            var alpha = 1 - p;
                            Glow.color = Color.white.AlphaMultiplied(alpha);
                            if (alpha < 0.75f)
                                LoadText.color = Color.white.AlphaMultiplied(alpha);
                            yield return null;
                        }
                    }

                    #endregion

                    break;
                }
            }
            
            #endregion

            #region Initialize Among Us Translation

            yield return LoadAmongUsTranslation();

            #endregion

            #region Download Depends

            CheckForListResources_Remove(ref remoteDependList, FileType.Depends);
            yield return DownloadListResources(remoteDependList, FileType.Depends);

            if (!ReloadLanguage)
            {
                CheckForListResources_Remove(ref remoteLanguageList, FileType.Languages);
            }
            
            yield return DownloadListResources(remoteLanguageList, FileType.Languages);

            #endregion

            if (!fastboot)
            {
                #region After Download Depends

                if (writeinVer) Main.LastStartVersion.Value = thisversion;
                Init();
                if (TranslationController.Instance.currentLanguage.languageID is not SupportedLangs.English)
                {
                    yield return FadeLoadText(false);
                    LoadText.text = GetString("Loading");
                    LoadText.color = Color.white;
                    yield return FadeLoadText(true);
                }

                yield return new WaitForSeconds(1f);

                #endregion
            }

            #region Check for resources

            if (!fastboot)
            {
                ProcessText.text = GetString("CheckingForFiles");
                ProcessText.color = Color.blue.AlphaMultiplied(0.75f);

                yield return FadeProcessText(true);
            }

            CheckForListResources_Remove(ref remoteImageList, FileType.Images);

            for (var i = remoteModNewsList.Count - 1; i >= 0; i--)
            {
                var resource = remoteModNewsList[i];
                remoteModNewsList.Remove(resource);

                foreach (var lang in EnumHelper.GetAllNames<SupportedLangs>())
                {
                    var file = $"{lang}/{resource}";

                    var localFilePath = PathManager.GetResourceFilesPath(FileType.ModNews, file);
                    if (File.Exists(localFilePath)) continue;
                    remoteModNewsList.Add(file);
                    XtremeLogger.Warn($"File do not exists: {localFilePath}", "Check");
                }
            }
            
            #endregion

            #region Download Resources
            
            var process = 0;
            
            if (!fastboot)
            {
                yield return new WaitForSeconds(0.5f);
                
                if (remoteImageList.Count > 0 || remoteModNewsList.Count > 0)
                {
                    yield return FadeProcessText(false);

                    ProcessText.color = ColorHelper.DownloadYellow;
                    ProcessText.text = GetString("DownloadingResources") +
                                       $"({process}/{remoteImageList.Count + remoteModNewsList.Count})";
                    yield return FadeProcessText(true);
                }
            }

            Action downloadAction = fastboot
                ? null
                : () => 
                {
                    process++;
                    ProcessText.text = GetString("DownloadingResources") +
                                       $"({process}/{remoteImageList.Count + remoteModNewsList.Count})";
                };
                
            yield return DownloadListResources(remoteImageList, FileType.Images, downloadAction);
            yield return DownloadListResources(remoteModNewsList, FileType.ModNews, downloadAction);
            
            if (!fastboot)
            {
                if (process > 0)
                {
                    ProcessText.color = ColorHelper.DownloadYellow;
                    yield return FadeProcessText(false);
                    ProcessText.color = ColorHelper.DownloadYellow;
                    ProcessText.text = GetString("DownLoadSucceedNotice");
                    yield return FadeProcessText(true);
                }

                yield return new WaitForSeconds(0.5f);

                yield return FadeProcessText(false);
            }

            #endregion

            if (!fastboot)
            {
                #region Load Complete
                
                yield return new WaitForSeconds(1f);
            
                Color green = ColorHelper.LoadCompleteGreen;
                LoadText.color = green.AlphaMultiplied(0.75f);
                LoadText.text = GetString("LoadingComplete");
            
                for (var i = 0; i < 3; i++)
                {
                    LoadText.gameObject.SetActive(false);
                    yield return new WaitForSeconds(0.03f);
                    LoadText.gameObject.SetActive(true);
                    yield return new WaitForSeconds(0.03f);
                }

                yield return new WaitForSeconds(0.5f);

                p = 1f;
                while (p > 0f)
                {
                    p -= Time.deltaTime * 1.2f;

                    Glow.color = Color.white.AlphaMultiplied(p);
                    Modlogo.color = Color.white.AlphaMultiplied(p);
                    if (p >= 0.75f)
                        LoadText.color = green.AlphaMultiplied(p - 0.75f);
                    yield return null;
                }

                GameObject.Destroy(LoadText.gameObject);
                GameObject.Destroy(ProcessText.gameObject);
                GameObject.Destroy(Modlogo.gameObject);
                GameObject.Destroy(ModlogoBlurred.gameObject);
                GameObject.Destroy(Teamlogo.gameObject);
                GameObject.Destroy(Glow.gameObject);

                #endregion
            }
            

            __instance.sceneChanger.BeginLoadingScene();
            __instance.doneLoadingRefdata = true;
        }

        private static void CheckForListResources_Remove(ref List<string> targetList, FileType fileType, Action action = null)
        {
            for (var i = targetList.Count - 1; i >= 0; i--)
            {
                action?.Invoke();
                var resource = targetList[i];
                var localFilePath = PathManager.GetLocalFilePath(fileType, resource);
                if (File.Exists(localFilePath))
                {
                    targetList.Remove(resource);
                }
                else
                {
                    XtremeLogger.Warn($"File do not exists: {localFilePath}", "Check");
                }
            }
        }

        private static IEnumerator DownloadListResources(List<string> targetList, FileType fileType, Action action = null)
        {
            foreach (var resource in targetList)
            {
                action?.Invoke();
                var task = StartDownload(fileType, resource);
                while (!task.IsCompleted)
                {
                    yield return null;
                }

                if (task.IsFaulted)
                {
                    XtremeLogger.Error($"Download of {resource} failed: {task.Exception}", "Download Resource");
                }
            }
        }

        private static IEnumerator FadeProcessText(bool show)
        {
            var cur = ProcessText.color;
            var p = 0.75f;
            while (p > 0)
            {
                p -= Time.deltaTime * 2.8f;
                var alpha = show ? 0.75f - p : p;
                ProcessText.color = cur.AlphaMultiplied(alpha);
                yield return null;
            }
        }
        
        private static IEnumerator FadeLoadText(bool show)
        {
            var cur = LoadText.color;
            var p = 0.75f;
            while (p > 0)
            {
                p -= Time.deltaTime * 2.8f;
                var alpha = show ? 0.75f - p : p;
                LoadText.color = cur.AlphaMultiplied(alpha);
                yield return null;
            }
        }

        private static IEnumerator LoadAmongUsTranslation()
        {
            yield return DestroyableSingleton<ReferenceDataManager>.Instance.Initialize();
            try
            {
                DestroyableSingleton<TranslationController>.Instance.Initialize();
            }
            catch
            {
                //
            }
        }
    }
    
    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
    internal class SplashLogoAnimatorPatch
    {
        public static void Prefix(SplashManager __instance)
        {
            if (!SkipLoadAnima) return;
            __instance.sceneChanger.AllowFinishLoadingScene();
            __instance.startedSceneLoad = true;
        }
    }
}
