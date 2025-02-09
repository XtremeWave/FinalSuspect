﻿using System.Collections;
using System.IO;
using BepInEx.Unity.IL2CPP.Utils;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Modules.Resources;
using TMPro;
using UnityEngine;
using static FinalSuspect.Modules.Resources.ResourcesDownloader;
using ListStr = System.Collections.Generic.List<string>;

namespace FinalSuspect.Patches.System;


public class LoadPatch
{
    
    static TextMeshPro loadText = null!;
    static TextMeshPro processText = null!;
    public static bool ReloadLanguage;
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
            
            #region Resources and variables
            float p;
            ListStr PreReady_remoteImageList =
            [
                "FinalSuspect-Logo.png",
                "FinalSuspect-Logo-Blurred.png",
                "LastResult-BG.png",
                "TeamLogo.png"
            ];
            ListStr remoteImageList =
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
            ListStr remoteDependList =
            [
                "YamlDotNet.dll",
                "YamlDotNet.xml"
            ];
            ListStr remoteModNewsList =
            [
                "FS.v1.0_20250129.txt",
                "FeaturesIntroduction.v1.0.txt"
            ];
            ListStr remoteLanguageList =
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
            var thisversion =
                $"{Main.PluginVersion}|{Main.DisplayedVersion}|{ThisAssembly.Git.Commit}-{ThisAssembly.Git.Branch}";
            var writeinVer = false;
            ReloadLanguage = thisversion != Main.LastStartVersion.Value 
                             && !File.Exists(PathManager.GetBypassFileType(FileType.Languages, BypassType.Once)) 
                             && !File.Exists(PathManager.GetBypassFileType(FileType.Languages, BypassType.Longterm));
            var reloadBypassPath_Once = PathManager.GetBypassFileType(FileType.Languages, BypassType.Once);
            var reloadBypassPath_Longterm = PathManager.GetBypassFileType(FileType.Languages, BypassType.Longterm);
            
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


            var fastboot = false;//Main.FastBoot.Value && !ReloadLanguage;
            #endregion
            var LogoAnimator = GameObject.Find("LogoAnimator");
            LogoAnimator.SetActive(false);
            
            #region First Start Final Suspect
            loadText = GameObject.Instantiate(__instance.errorPopup.InfoText, null);
            loadText.transform.localPosition = new(0f, -0.28f, -10f);
            loadText.fontStyle = FontStyles.Bold;
            loadText.text = null;
            for (var i = PreReady_remoteImageList.Count - 1; i >= 0; i--)
            {
                fastboot = false;
                var resource = PreReady_remoteImageList[i];
                var localFilePath = PathManager.GetResourceFilesPath(FileType.Images, resource);
                if (File.Exists(localFilePath))
                {
                    PreReady_remoteImageList.Remove(resource);
                }
                else
                {
                    XtremeLogger.Warn($"File do not exists: {localFilePath}", "Check");
                }
            }
            foreach (var resource in PreReady_remoteImageList)
            {
                loadText.text = $"Welcome to <color={ColorHelper.ModColor}>FinalSuspect</color>.";
                var task = StartDownload(FileType.Images, resource);
                while (!task.IsCompleted)
                {
                    yield return null; 
                }

                if (task.IsFaulted)
                {
                    XtremeLogger.Error($"Download of {resource} failed: {task.Exception}", "Download Resource");
                }

            }

            if (!string.IsNullOrEmpty(loadText.text))
            {
                p = 1f;
                while (p > 0f)
                {
                    p -= Time.deltaTime * 2.8f;
                    loadText.color = Color.white.AlphaMultiplied(p);
                    yield return null;
                }
                yield return new WaitForSeconds(2f);
            }
            

            #endregion

            if (!fastboot)
            {
                #region Team Logo Anima
                var teamlogo = ObjectHelper.CreateObject<SpriteRenderer>("Team_Logo", null, new Vector3(0, 0f, -5f));
                teamlogo.sprite = Utils.LoadSprite("TeamLogo.png", 120f);
            
                p = 1f;
                while (p > 0f)
                {
                    p -= Time.deltaTime * 2.8f;
                    var alpha = 1 - p;
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
#endregion
            }
            
            #region Create Mod Logo
            var modlogo = ObjectHelper.CreateObject<SpriteRenderer>("Mod_Logo", null, new Vector3(0, 0.3f, -5f));
            var modlogo_Blurred = ObjectHelper.CreateObject<SpriteRenderer>("Mod_Logo_Blurred", null, new Vector3(0, 0.3f, -5f));
            modlogo.sprite = Utils.LoadSprite("FinalSuspect-Logo.png", 150f);
            modlogo_Blurred.sprite = Utils.LoadSprite("FinalSuspect-Logo-Blurred.png", 150f);
            var glow = ObjectHelper.CreateObject<SpriteRenderer>("glow", null, new Vector3(0, 0.3f, -5f));
            glow.sprite = Utils.LoadSprite("FinalSuspect-Logo.png");
            glow.color = Color.white.AlphaMultiplied(0f);
            #endregion

            #region Start Load
            p = 1f;
            while (p > 0f)
            {
                p -= Time.deltaTime * 2.8f;
                var alpha = 1 - p;
                if (fastboot)
                    glow.color = Color.white.AlphaMultiplied(alpha);
                modlogo.color = Color.white.AlphaMultiplied(alpha);
                modlogo_Blurred.color = Color.white.AlphaMultiplied(Mathf.Min(1f, alpha * (p * 2)));
                modlogo.transform.localScale = Vector3.one * (p * p * 0.012f + 1f);
                modlogo_Blurred.transform.localScale = Vector3.one * (p * p * 0.012f + 1f);
                yield return null;
            }
            
            modlogo.color = Color.white;
            modlogo_Blurred.gameObject.SetActive(false);
            modlogo.transform.localScale = Vector3.one;
            if (!fastboot)
                yield return new WaitForSeconds(0.75f);
            
            if (!fastboot)
            {
                
                loadText.color = Color.white.AlphaMultiplied(0.5f);
                loadText.text = "Loading...";
                p = 1f;
                while (p > 0)
                {
                    p -= Time.deltaTime * 2.8f;
                    var alpha = 1 - p;
                    glow.color = Color.white.AlphaMultiplied(alpha);
                    if (alpha < 0.5f)
                        loadText.color = Color.white.AlphaMultiplied(alpha);
                    yield return null;
                }
            }
            #endregion
            
            #region Initialize Among Us Translation
            yield return DestroyableSingleton<ReferenceDataManager>.Instance.Initialize();
            try
            {
                DestroyableSingleton<TranslationController>.Instance.Initialize();
            }
            catch
            { }
            #endregion

            #region Download Depends
            if (!fastboot)
            {
                for (var i = remoteDependList.Count - 1; i >= 0; i--)
                {
                    var resource = remoteDependList[i];
                    var localFilePath = PathManager.GetLocalPath(LocalType.BepInEx) + resource;
                    if (File.Exists(localFilePath))
                    {
                        remoteDependList.Remove(resource);
                    }
                    else
                    {
                        XtremeLogger.Warn($"File do not exists: {localFilePath}", "Check");
                    }
                }

                foreach (var resource in remoteDependList)
                {
                    var task = StartDownload(FileType.Depends, resource);
                    while (!task.IsCompleted)
                    {
                        yield return null;
                    }

                    if (task.IsFaulted)
                    {
                        XtremeLogger.Error($"Download of {resource} failed: {task.Exception}", "Download Resource");
                    }
                }
                

                if (!ReloadLanguage)
                {
                    for (var i = remoteLanguageList.Count - 1; i >= 0; i--)
                    {
                        var resource = remoteLanguageList[i];
                        var localFilePath = PathManager.GetResourceFilesPath(FileType.Languages, resource);
                        if (File.Exists(localFilePath))
                        {
                            remoteLanguageList.Remove(resource);
                        }
                        else
                        {
                            XtremeLogger.Warn($"File do not exists: {localFilePath}", "Check");
                        }

                    }
                }

                foreach (var resource in remoteLanguageList)
                {
                    var task = StartDownload(FileType.Languages, resource);
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
            #endregion

            #region After Download Depends
            if (writeinVer)
                Main.LastStartVersion.Value = thisversion;
            Init();
            if (TranslationController.Instance.currentLanguage.languageID is not SupportedLangs.English)
            {
                p = 1f;
                while (p > 0)
                {
                    p -= Time.deltaTime * 2.8f;
                    var alpha = p;
                    if (alpha < 0.5f)
                        loadText.color = Color.white.AlphaMultiplied(alpha);
                    yield return null;
                }

                loadText.text = GetString("Loading");
                p = 1f;
                while (p > 0)
                {
                    p -= Time.deltaTime * 2.8f;
                    var alpha = 1 - p;
                    if (alpha < 0.5f)
                        loadText.color = Color.white.AlphaMultiplied(alpha);
                    yield return null;
                }
            }
            yield return new WaitForSeconds(1f);
            #endregion
            
            #region Check for resources
            processText = GameObject.Instantiate(__instance.errorPopup.InfoText, null);
            processText.transform.localPosition = new(0f, -0.7f, -10f);
            processText.fontStyle = FontStyles.Bold;
            processText.text = GetString("CheckingForFiles");
            processText.color = Color.blue.AlphaMultiplied(0.5f);
            p = 1f;
            while (p > 0)
            {
                p -= Time.deltaTime * 2.8f;
                var alpha = 1-p;
                if (alpha < 0.5f)
                    processText.color = Color.blue.AlphaMultiplied(alpha);
                yield return null;
            }
            
            while (!VersionChecker.isChecked)
            {
                yield return null;
            }
            
            for (var i = remoteImageList.Count - 1; i >= 0; i--)
            {
                var resource = remoteImageList[i];
                var localFilePath = PathManager.GetResourceFilesPath(FileType.Images, resource);
                var task = IsUrl404Async(FileType.Images, resource);
                while (!task.IsCompleted)
                {
                    yield return null; 
                }
                if (File.Exists(localFilePath) || task.Result)
                {
                    remoteImageList.Remove(resource);
                }
                else
                {
                    XtremeLogger.Warn($"File do not exists: {localFilePath}", "Check");
                }
            }
            
            for (var i = remoteModNewsList.Count - 1; i >= 0; i--)
            {
                var resource = remoteModNewsList[i];
                remoteModNewsList.Remove(resource);
                
                foreach (var lang in EnumHelper.GetAllNames<SupportedLangs>())
                {
                    var file = $"{lang}/{resource}";
                    var task = IsUrl404Async(FileType.Images, file);
                    while (!task.IsCompleted)
                    {
                        yield return null; 
                    }
                    var localFilePath = PathManager.GetResourceFilesPath(FileType.ModNews, file);
                    if (!File.Exists(localFilePath) && !task.Result)
                    {
                        remoteModNewsList.Add(file);
                        XtremeLogger.Warn($"File do not exists: {localFilePath}", "Check");
                    }
                }
            }
            

            #endregion
            
            #region Download Resources
            yield return new WaitForSeconds(0.5f);

            var process = 0;
            if (remoteImageList.Count > 0 || remoteModNewsList.Count > 0)
            {
                p = 1f;
                while (p > 0)
                {
                    p -= Time.deltaTime * 2.8f;
                    var alpha = p;
                    if (alpha < 0.5f)
                        processText.color = Color.blue.AlphaMultiplied(alpha);
                    yield return null;
                }
                Color yellow = ColorHelper.DownloadYellow;
                processText.text = GetString("DownloadingResources") + $"({process}/{remoteImageList.Count + remoteModNewsList.Count})";
                p = 1f;
                while (p > 0)
                {
                    
                    p -= Time.deltaTime * 2.8f;
                    var alpha = 1-p;
                    if (alpha < 0.5f)
                        processText.color = yellow.AlphaMultiplied(alpha);
                    yield return null;
                }

            }

            foreach (var resource in remoteImageList)
            {
                process++;
                processText.text = GetString("DownloadingResources") + $"({process}/{remoteImageList.Count + remoteModNewsList.Count})";
                var task = StartDownload(FileType.Images, resource);
                while (!task.IsCompleted)
                {
                    yield return null; 
                }

                if (task.IsFaulted)
                {
                    XtremeLogger.Error($"Download of {resource} failed: {task.Exception}", "Download Resource");
                }
            }
            foreach (var resource in remoteModNewsList)
            {
                process++;
                processText.text = GetString("DownloadingResources") + $"({process}/{remoteImageList.Count + remoteModNewsList.Count})";
                var task = StartDownload(FileType.ModNews, resource);

                while (!task.IsCompleted)
                {
                    yield return null; 
                }

                if (task.IsFaulted)
                {
                    XtremeLogger.Error($"Download of {resource} failed: {task.Exception}", "Download Resource");
                }
            }

            if (process > 0)
            {
                Color yellow = ColorHelper.DownloadYellow;
                p = 1f;
                while (p > 0)
                {
                    p -= Time.deltaTime * 2.8f;
                    var alpha = p;
                    if (alpha < 0.5f)
                        processText.color = yellow.AlphaMultiplied(alpha);
                    yield return null;
                }
                processText.text = GetString("DownLoadSucceedNotice");
                p = 1f;
                while (p > 0)
                {
                    p -= Time.deltaTime * 2.8f;
                    var alpha = 1 - p;
                    if (alpha < 0.5f)
                        processText.color = yellow.AlphaMultiplied(alpha);
                    yield return null;
                }
               
            }

            yield return new WaitForSeconds(0.5f);
            var cur = processText.color;
            p = 1f;
            while (p > 0)
            {
                p -= Time.deltaTime * 2.8f;
                var alpha = p;
                if (alpha < 0.5f)
                    processText.color = cur.AlphaMultiplied(alpha);
                yield return null;
            }
            GameObject.Destroy(processText.gameObject);
            #endregion

            #region Load Complete
            yield return new WaitForSeconds(1f);
            Color green = ColorHelper.LoadCompleteGreen;
            loadText.color = green.AlphaMultiplied(0.5f);

            loadText.text = GetString("LoadingComplete");
            for (var i = 0; i < 3; i++)
            {
                loadText.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.03f);
                loadText.gameObject.SetActive(true);
                yield return new WaitForSeconds(0.03f);
            }
            yield return new WaitForSeconds(0.5f);

            p = 1f;
            while (p > 0f)
            {                
                p -= Time.deltaTime * 1.2f;
                
                glow.color = Color.white.AlphaMultiplied(p);
                modlogo.color = Color.white.AlphaMultiplied(p);
                if (p >= 0.5f) 
                    loadText.color = green.AlphaMultiplied(p - 0.5f);
                yield return null;
            }
            GameObject.Destroy(loadText.gameObject);
            modlogo.color = Color.clear;
            #endregion
            __instance.loadingObject.SetActive(false);
            __instance.sceneChanger.BeginLoadingScene();
            __instance.doneLoadingRefdata = true;
            
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