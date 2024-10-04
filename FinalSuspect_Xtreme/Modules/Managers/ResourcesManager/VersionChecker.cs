using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;
using System.Collections.Generic;
using static FinalSuspect.Translator;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using FinalSuspect.Attributes;

namespace FinalSuspect.Modules.Managers.ResourcesManager;

public static class VersionChecker
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPriority(Priority.LowerThanNormal)]
    public class Start
    {
        public static void Postfix()
        {
            CustomPopup.Init();
            if (!isChecked && firstStart) CheckForUpdate();
            ModUpdater.SetUpdateButtonStatus();
            firstStart = false;
        }
    }

    private static IReadOnlyList<string> URLs => new List<string>
    {
#if DEBUG
        $"file:///{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "fs_info.json")}",
#else
        "https://raw.githubusercontent.com/XtremeWave/FinalSuspect/FinalSus/fs_info.json",
        "https://gitee.com/XtremeWave/FinalSuspect/raw/FinalSus/fs_info.json",
#endif
    };
    private static IReadOnlyList<string> GetInfoFileUrlList()
    {
        var list = URLs.ToList();
        if (IsChineseUser) list.Reverse();
        return list;
    }

    public static bool firstStart = true;

    public static bool hasUpdate = false;
    public static bool forceUpdate = false;
    public static bool isBroken = false;
    public static bool isChecked = false;
    public static bool DebugUnused = false;
    public static string versionInfoRaw = "";

    public static Version latestVersion = null;
    public static string showVer = "";
    public static Version DebugVer = null;
    public static bool CanUpdate = false;
    public static string verHead = "";
    public static string verDate = "";
    public static Version minimumVersion = null;
    public static int creation = 0;
    public static string md5 = "";
    public static bool IsSupported { get; private set; } = true;
    public static int visit => isChecked ? 216822 : 0;

    private static int retried = 0;
    private static bool firstLaunch = true;

    public static string GithubUrl;
    public static string GiteeUrl;
    public static string ObjectStorageUrl;
    public static string AUModSiteUrl;

    public static void Check()
    {
        var amongUsVersion = Version.Parse(Application.version);
        var lowestSupportedVersion = Version.Parse(Main.LowestSupportedVersion);
        IsSupported = amongUsVersion >= lowestSupportedVersion;
        if (!IsSupported)
        {
            ErrorText.Instance.AddError(ErrorCode.UnsupportedVersion);
        }
    }

    public static void Retry()
    {
        retried++;
        CustomPopup.Show(GetString("updateCheckPopupTitle"), GetString("PleaseWait"), null);
        _ = new LateTask(CheckForUpdate, 0.3f, "Retry Check Update");
    }
    [PluginModuleInitializer]
    public static void CheckForInit()
    {
        isChecked = false;

        foreach (var url in GetInfoFileUrlList())
        {
            if (GetVersionInfo(url).GetAwaiter().GetResult())
            {
                isChecked = true;
                break;
            }
        }

        Logger.Msg("Check For Update: " + isChecked, "CheckRelease");
        isBroken = !isChecked;
        if (isChecked)
        {
            Logger.Info("Has Update: " + hasUpdate, "CheckRelease");
            Logger.Info("Latest Version: " + latestVersion.ToString(), "CheckRelease");
            Logger.Info("Minimum Version: " + minimumVersion.ToString(), "CheckRelease");
            Logger.Info("Creation: " + creation.ToString(), "CheckRelease");
            Logger.Info("Force Update: " + forceUpdate, "CheckRelease");
            Logger.Info("File MD5: " + md5, "CheckRelease");
            Logger.Info("Github Url: " + ModUpdater.downloadUrl_github, "CheckRelease");
            Logger.Info("Gitee Url: " + ModUpdater.downloadUrl_gitee, "CheckRelease");
            Logger.Info("Website Url: " + ModUpdater.downloadUrl_objectstorage, "CheckRelease");
            Logger.Info("Announcement (English): " + ModUpdater.announcement_en, "CheckRelease");
            Logger.Info("Announcement (SChinese): " + ModUpdater.announcement_zh, "CheckRelease");

        }

    }
    public static void CheckForUpdate()
    {
        isChecked = false;
        ModUpdater.DeleteOldFiles();

        foreach (var url in GetInfoFileUrlList())
        {
            if (GetVersionInfo(url).GetAwaiter().GetResult())
            {
                isChecked = true;
                break;
            }
        }

        Logger.Msg("Check For Update: " + isChecked, "CheckRelease");
        isBroken = !isChecked;
        if (isChecked)
        {
            Logger.Info("Has Update: " + hasUpdate, "CheckRelease");
            Logger.Info("Latest Version: " + latestVersion.ToString(), "CheckRelease");
            Logger.Info("Minimum Version: " + minimumVersion.ToString(), "CheckRelease");
            Logger.Info("Creation: " + creation.ToString(), "CheckRelease");
            Logger.Info("Force Update: " + forceUpdate, "CheckRelease");
            Logger.Info("File MD5: " + md5, "CheckRelease");
            Logger.Info("Github Url: " + ModUpdater.downloadUrl_github, "CheckRelease");
            Logger.Info("Gitee Url: " + ModUpdater.downloadUrl_gitee, "CheckRelease");
            Logger.Info("Website Url: " + ModUpdater.downloadUrl_objectstorage, "CheckRelease");
            Logger.Info("Announcement (English): " + ModUpdater.announcement_en, "CheckRelease");
            Logger.Info("Announcement (SChinese): " + ModUpdater.announcement_zh, "CheckRelease");

            if (firstLaunch || isBroken)
            {
                firstLaunch = false;
                var annos = IsChineseUser ? ModUpdater.announcement_zh : ModUpdater.announcement_en;
                if (isBroken) CustomPopup.Show(GetString(StringNames.AnnouncementLabel), annos, new() { (GetString(StringNames.ExitGame), Application.Quit) });
                else CustomPopup.Show(GetString(StringNames.AnnouncementLabel), annos, new() { (GetString(StringNames.Okay), null) });
            }
        }
        else
        {
            if (retried >= 2) CustomPopup.Show(GetString("updateCheckPopupTitle"), GetString("updateCheckFailedExit"), new() { (GetString(StringNames.Okay), null) });
            else CustomPopup.Show(GetString("updateCheckPopupTitle"), GetString("updateCheckFailedRetry"), new() { (GetString("Retry"), Retry) });
        }

        ModUpdater.SetUpdateButtonStatus();
    }
    public static async Task<bool> GetVersionInfo(string url)
    {
        Logger.Msg(url, "CheckRelease");
        try
        {
            string result;
            if (url.StartsWith("file:///"))
            {
                result = File.ReadAllText(url[8..]);
            }
            else
            {
                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("User-Agent", "FinalSuspect Updater");
                client.DefaultRequestHeaders.Add("Referer", "www.xtreme.net.cn");
                using var response = await client.GetAsync(new Uri(url), HttpCompletionOption.ResponseContentRead);
                if (!response.IsSuccessStatusCode || response.Content == null)
                {
                    Logger.Error($"Failed: {response.StatusCode}", "CheckRelease");
                    return false;
                }
                result = await response.Content.ReadAsStringAsync();
                result = result.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
            }

            JObject data = JObject.Parse(result);

            verHead = new(data["verHead"]?.ToString());

            DebugVer = new(data["DebugVer"]?.ToString());


            CanUpdate = bool.Parse(new(data["CanUpdate"]?.ToString()));



            verDate = new(data["verDate"]?.ToString());
            md5 = data["md5"]?.ToString();
            latestVersion = new(data["version"]?.ToString());

            showVer = $"{verHead}_{verDate}";

            var minVer = data["minVer"]?.ToString();
            minimumVersion = minVer.ToLower() == "latest" ? latestVersion : new(minVer);
            creation = int.Parse(data["creation"]?.ToString());
            isBroken = data["allowStart"]?.ToString().ToLower() != "true";

            JObject announcement = data["announcement"].Cast<JObject>();
            ModUpdater.announcement_en = announcement["English"]?.ToString();
            ModUpdater.announcement_zh = announcement["SChinese"]?.ToString();

            JObject downloadUrl = data["url"].Cast<JObject>();

            GithubUrl = downloadUrl["githubUrl"]?.ToString();
            GiteeUrl = downloadUrl["giteeUrl"]?.ToString();
            ObjectStorageUrl = downloadUrl["objectstorageUrl"]?.ToString();
            AUModSiteUrl = downloadUrl["aumodsiteUrl"]?.ToString();

            hasUpdate = Main.version < latestVersion;
            forceUpdate = Main.version < minimumVersion || creation > Main.PluginCreation;
#if DEBUG
            DebugUnused = Main.version < DebugVer;
            hasUpdate = forceUpdate = DebugUnused;
#endif

            return true;
        }
        catch
        {
            return false;
        }
    }

}