using System;
using System.Collections;
using System.IO;
using BepInEx.Unity.IL2CPP.Utils;
using FinalSuspect.ClientActions.FeatureItems.MainMenuStyle;
using FinalSuspect.ClientActions.FeatureItems.MyMusic;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Resources;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(SplashManager))]
public static class LoadPatch
{
    private static bool _reloadLanguage;
    private static bool _skipLoadAnimation;
    private static bool _firstLaunch;

    [HarmonyPatch(nameof(SplashManager.Start)), HarmonyPrefix]
    public static bool Start(SplashManager __instance)
    {
        __instance.startTime = Time.time;
        __instance.StartCoroutine(InitializeRefData(__instance));
        return false;
    }

    private static IEnumerator InitializeRefData(SplashManager instance)
    {
        CreateTextComponents(instance);
        yield return HandleFirstLaunch();
        CreateLogoComponents();
        yield return HandleCoreLoadingProcess();
        instance.sceneChanger.BeginLoadingScene();
        instance.doneLoadingRefdata = true;
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
            /* Ignored */
        }
    }


    #region Harmony Patches

    [HarmonyPatch(nameof(SplashManager.Update)), HarmonyPrefix]
    public static void Update(SplashManager __instance)
    {
        if (!_skipLoadAnimation) return;
        __instance.sceneChanger.AllowFinishLoadingScene();
        __instance.startedSceneLoad = true;
    }

    #endregion

    #region Initialization Helpers

    private static void CreateTextComponents(SplashManager instance)
    {
        _loadText = CreateTextComponent(instance, new Vector3(0f, -0.28f, -10f));
        _processText = CreateTextComponent(instance, new Vector3(0f, -0.7f, -10f));
    }

    private static TextMeshPro CreateTextComponent(SplashManager instance, Vector3 position)
    {
        var text = Object.Instantiate(instance.errorPopup.InfoText, null);
        text.transform.localPosition = position;
        text.fontStyle = FontStyles.Bold;
        text.text = string.Empty;
        return text;
    }

    private static void CreateLogoComponents()
    {
        _authorLogo =
            ObjectHelper.CreateSpriteRenderer("Author_Logo", "AuthorLogo1.png", 120f, new Vector3(0, 0f, -5f));
        _modLogo = ObjectHelper.CreateSpriteRenderer("Mod_Logo", "FinalSuspect-Logo.png", 150f,
            new Vector3(0, 0.3f, -5f));
        _modLogoBlurred = ObjectHelper.CreateSpriteRenderer("Mod_Logo_Blurred", "FinalSuspect-Logo-Blurred.png",
            150f,
            new Vector3(0, 0.3f, -5f));
        _glow = ObjectHelper.CreateSpriteRenderer("Glow", "FinalSuspect-Logo.png", 1f, new Vector3(0, 0.3f, -5f));
    }

    #endregion

    #region Loading Process

    private static IEnumerator HandleFirstLaunch()
    {
        var logoAnimator = GameObject.Find("LogoAnimator");
        logoAnimator.SetActive(false);


        CheckForListResources(ref ResourcesHelper.PreReadyRemoteImageList, FileType.Images);
        CheckForListResources(ref ResourcesHelper.PreReadyRemoteMusicList, FileType.Musics);
        yield return DownloadResources(ResourcesHelper.PreReadyRemoteImageList, FileType.Images,
            HandleFirstLaunchText, true);
        yield return DownloadResources(ResourcesHelper.PreReadyRemoteMusicList, FileType.Musics,
            HandleFirstLaunchText, true);
        if (string.IsNullOrEmpty(_loadText.text)) yield break;
        _loadText.text = string.Empty;
        _firstLaunch = true;
        yield return new WaitForSeconds(2f);
    }

    private static void HandleFirstLaunchText()
    {
        _loadText.text = $"Welcome to <color={ColorHelper.FSColorHex}>FinalSuspect</color>.";
    }

    private static IEnumerator HandleCoreLoadingProcess()
    {
        var fastLaunchMode = CheckFastLaunchModeCondition() && !_firstLaunch;
        Main.FastLaunchMode.Value = fastLaunchMode;

        yield return fastLaunchMode ? HandleFastLaunchMode() : HandleNormalBoot();
        yield return LoadEssentialResources();
        yield return HandlePostDownloadProcess(fastLaunchMode);
    }

    private static bool CheckFastLaunchModeCondition()
    {
        var currentVersion = $"{Main.PluginVersion}|{Main.DisplayedVersion}|{Main.GitCommit}-{Main.GitBranch}";
        var bypassType = Main.LanguageUpdateBypass.Value;

        _reloadLanguage = currentVersion != Main.LastStartVersion.Value
                          && bypassType == BypassType.Dont;

        switch (bypassType)
        {
            case BypassType.Dont:
                Main.LastStartVersion.Value = currentVersion;
                break;
            case BypassType.Once:
                Main.LanguageUpdateBypass.Value = BypassType.Dont;
                break;
            case BypassType.LongTerm:
            default:
                break;
        }

        return Main.FastLaunchMode.Value && !_reloadLanguage;
    }

    #endregion

    #region Boot Handlers

    private static IEnumerator HandleFastLaunchMode()
    {
        SetFastLaunchModeVisuals();
        TranslatorInit();
        AudioManager.ReloadTag();
        var style = MainMenuStyleManager.MainMenuStyles[Main.CurrentStyleId.Value];
        var audio = FinalMusic.musics.FirstOrDefault(x => x.CurrentAudio == style.MainMenuMusic);
        if (audio != null)
            AudioPlayer.Play(audio, true);


        UpdateProcessText(GetString("ClientOption.FastLaunchMode"), Color.green);
        yield return new WaitForSeconds(1f);
        _skipLoadAnimation = true;
    }

    private static void SetFastLaunchModeVisuals()
    {
        _authorLogo.color = Color.white;
        _authorLogo.transform.localPosition = new Vector3(0, 2.4f, -5f);
        _authorLogo.transform.localScale = new Vector3(0.55f, 0.55f, 1f);

        _modLogo.color = Color.white;
        _modLogo.transform.localPosition = new Vector3(0, 0, -5f);
        _modLogo.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

        _glow.color = Color.green;
    }

    private static IEnumerator HandleNormalBoot()
    {
        yield return AnimateAuthorLogo();
        yield return AnimateModLogo();
        yield return ShowLoadingProgress();
    }

    #endregion

    #region Animation Coroutines

    private static IEnumerator AnimateAuthorLogo()
    {
        _authorLogo.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        yield return FadeSprite(_authorLogo, 2.8f, false);
        yield return new WaitForSeconds(1.5f);
        yield return FadeSprite(_authorLogo, 2.8f, true);
        yield return new WaitForSeconds(2f);
    }

    private static IEnumerator AnimateModLogo()
    {
        var progress = 1f;
        while (progress > 0f)
        {
            progress -= Time.deltaTime * 2.8f;
            var alpha = 1 - progress;

            _modLogo.color = Color.white.AlphaMultiplied(alpha);
            _modLogoBlurred.color = Color.white.AlphaMultiplied(Mathf.Min(1f, alpha * (progress * 2)));

            var scale = Vector3.one * (progress * progress * 0.012f + 1f);
            _modLogo.transform.localScale = scale;
            _modLogoBlurred.transform.localScale = scale;

            yield return null;
        }

        _modLogo.color = Color.white;
        _modLogoBlurred.gameObject.SetActive(false);
        _modLogo.transform.localScale = Vector3.one;
        yield return new WaitForSeconds(0.75f);
    }

    private static IEnumerator ShowLoadingProgress()
    {
        _loadText.color = Color.white.AlphaMultiplied(0.75f);
        _loadText.text = "Loading...";

        var progress = 1f;
        while (progress > 0)
        {
            progress -= Time.deltaTime * 2.8f;
            var alpha = 1 - progress;
            _glow.color = Color.white.AlphaMultiplied(alpha);

            if (alpha < 0.75f)
                _loadText.color = Color.white.AlphaMultiplied(alpha);

            yield return null;
        }
    }

    #endregion

    #region Resource Management

    private static IEnumerator LoadEssentialResources()
    {
        yield return LoadAmongUsTranslation();
        CheckForListResources(ref ResourcesHelper.RemoteDependList, FileType.Depends);
        yield return DownloadResources(ResourcesHelper.RemoteDependList, FileType.Depends, null, true);

        List<string> RemoteLanguageList = [];
        RemoteLanguageList.AddRange(EnumHelper.GetAllNames<SupportedLangs>().Select(lang => lang + ".yaml"));

        if (!_reloadLanguage)
            CheckForListResources(ref RemoteLanguageList, FileType.Languages);

        if (RemoteLanguageList.Count > 0)
            yield return DownloadResources(RemoteLanguageList, FileType.Languages, null, true);

        if (!_skipLoadAnimation)
        {
            TranslatorInit();
            AudioManager.ReloadTag();
            var style = MainMenuStyleManager.MainMenuStyles[Main.CurrentStyleId.Value];
            var audio = FinalMusic.musics.FirstOrDefault(x => x.CurrentAudio == style.MainMenuMusic);
            if (audio != null)
                AudioPlayer.Play(audio, true);
        }
    }

    private static IEnumerator HandlePostDownloadProcess(bool fastLaunchMode)
    {
        if (fastLaunchMode) yield break;
        if (TranslationController.Instance.currentLanguage.languageID != SupportedLangs.English)
        {
            yield return FadeText(_loadText, false);
            _loadText.text = GetString("Tip.Loading");
            _loadText.color = Color.white;
            yield return FadeText(_loadText, true);
        }

        yield return new WaitForSeconds(1f);
        yield return VerifyAdditionalResources();
        yield return ShowLoadCompleteAnimation();
    }

    private static IEnumerator VerifyAdditionalResources()
    {
        UpdateProcessText(GetString("Tip.CheckingForFiles"), Color.blue.AlphaMultiplied(0.75f));
        yield return FadeText(_processText, true);

        CheckForListResources(ref ResourcesHelper.RemoteImageList, FileType.Images);

        if (ResourcesHelper.RemoteImageList.Count > 0)
            yield return HandleResourceDownloads();
        else
            yield return FadeText(_processText, false);
    }

    private static IEnumerator HandleResourceDownloads()
    {
        var downloadCount = ResourcesHelper.RemoteImageList.Count;
        var progress = 0;

        UpdateProcessText($"{GetString("Tip.Downloading")}({progress}/{downloadCount})",
            ColorHelper.DownloadYellow);
        yield return FadeText(_processText, true);

        var updateProgress = new Action(() =>
        {
            progress++;
            _processText.text = $"{GetString("Tip.Downloading")}({progress}/{downloadCount})";
        });

        yield return DownloadResources(ResourcesHelper.RemoteImageList, FileType.Images, updateProgress);
        yield return ShowDownloadCompletion();
    }

    private static IEnumerator ShowDownloadCompletion()
    {
        yield return FadeText(_processText, false);
        UpdateProcessText(GetString("Tip.DownLoadFinished"), ColorHelper.DownloadYellow);
        yield return FadeText(_processText, true);
        yield return new WaitForSeconds(0.5f);
        yield return FadeText(_processText, false);
    }

    #region Load Complete Animation

    private static IEnumerator ShowLoadCompleteAnimation()
    {
        yield return new WaitForSeconds(1f);

        Color green = ColorHelper.CompleteGreen;
        _loadText.color = green.AlphaMultiplied(0.75f);
        _loadText.text = GetString("Tip.LoadingComplete");

        for (var i = 0; i < 3; i++)
        {
            _loadText.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.03f);
            _loadText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.03f);
        }

        yield return new WaitForSeconds(0.5f);

        var progress = 1f;
        while (progress > 0f)
        {
            progress -= Time.deltaTime * 1.2f;
            _glow.color = Color.white.AlphaMultiplied(progress);
            _modLogo.color = Color.white.AlphaMultiplied(progress);

            if (progress >= 0.75f)
                _loadText.color = green.AlphaMultiplied(progress - 0.75f);

            yield return null;
        }

        Object.Destroy(_loadText.gameObject);
        Object.Destroy(_processText.gameObject);
        Object.Destroy(_modLogo.gameObject);
        Object.Destroy(_modLogoBlurred.gameObject);
        Object.Destroy(_authorLogo.gameObject);
        Object.Destroy(_glow.gameObject);
    }

    #endregion

    #endregion

    #region Utility Methods

    private static void CheckForListResources(ref List<string> targetList, FileType fileType)
    {
        for (var i = targetList.Count - 1; i >= 0; i--)
        {
            var resource = targetList[i];
            var path = GetLocalFilePath(fileType, resource);
            if (fileType is FileType.Musics)
                AudioManager.ConvertExtension(ref path);
            if (File.Exists(path))
                targetList.Remove(resource);
            else
                Warn($"File does not exist: {GetLocalFilePath(fileType, resource)}", "Check");
        }
    }

    private static IEnumerator DownloadResources(List<string> resources, FileType fileType,
        Action progressCallback = null, bool essential = false)
    {
        foreach (var resource in resources)
        {
            progressCallback?.Invoke();
            var task = ResourcesDownloader.StartDownload(fileType, resource);
            while (!task.IsCompleted) yield return null;

            if (!task.IsFaulted && task.Result) continue;

            Error($"Download failed: {resource} - {task.Exception}", "Download Resource");
            if (!essential) continue;
            yield return HandleDownloadError();
            Fatal("DOWNLOAD ESSENTIAL RESOURCES FAILED", "Download Resource");
        }
    }

    private static IEnumerator HandleDownloadError()
    {
        yield return FadeText(_loadText, false);
        _loadText.text = "Downloading essential resources failed, please restart the game\nRestart countdown: ";
        _loadText.color = Color.red;
        yield return FadeText(_loadText, true);

        var countdown = 10f;
        while (countdown > 0)
        {
            _loadText.text =
                $"Downloading essential resources failed, please restart the game\nRestart countdown: {countdown:F0}";
            countdown -= Time.deltaTime;
            yield return null;
        }

        Application.Quit();
    }

    private static IEnumerator FadeText(TextMeshPro text, bool show, float duration = 2.8f)
    {
        var progress = 0.75f;
        var originalColor = text.color;

        while (progress > 0)
        {
            progress -= Time.deltaTime * duration;
            var alpha = show ? 0.75f - progress : progress;
            text.color = originalColor.AlphaMultiplied(alpha);
            yield return null;
        }
    }

    private static IEnumerator FadeSprite(SpriteRenderer sprite, float speed, bool fadeOut)
    {
        var progress = 1f;
        while (progress > 0f)
        {
            progress -= Time.deltaTime * speed;
            var alpha = fadeOut ? progress : 1 - progress;
            sprite.color = Color.white.AlphaMultiplied(alpha);
            yield return null;
        }
    }

    private static void UpdateProcessText(string text, Color color)
    {
        _processText.text = text;
        _processText.color = color;
    }

    #endregion

    #region UI Components

    private static TextMeshPro _loadText = null!;
    private static TextMeshPro _processText = null!;
    private static SpriteRenderer _authorLogo = null!;
    private static SpriteRenderer _modLogo = null!;
    private static SpriteRenderer _modLogoBlurred = null!;
    private static SpriteRenderer _glow = null!;

    #endregion
}