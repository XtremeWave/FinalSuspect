using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Features;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace FinalSuspect.Modules.Resources;

public static class VersionChecker
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPriority(Priority.LowerThanNormal)]
    public class Start
    {
        public static void Postfix()
        {
            CustomPopup.Init();
            if (firstStart)
            {
                CheckForUpdate();
                SpamManager.Init();
            }
            ModUpdater.SetUpdateButtonStatus();
            firstStart = false;
        }
    }

    public static bool firstStart = true;

    public static bool hasUpdate;
    public static bool forceUpdate;
    public static bool isBroken;
    public static bool isChecked;
    public static bool DebugUnused = false;
    public static string versionInfoRaw = "";

    public static Version latestVersion;
    public static string showVer = "";
    public static Version DebugVer;
    public static bool CanUpdate;
    public static string verHead = "";
    public static string verDate = "";
    public static Version minimumVersion;
    public static int creation;
    public static string md5 = "";
    public static bool IsSupported { get; private set; } = true;

    private static int retried;
    private static bool firstLaunch = true;

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

    private static void Retry()
    {
        retried++;
        CustomPopup.Show(GetString("updateCheckPopupTitle"), GetString("PleaseWait"), null);
        _ = new LateTask(CheckForUpdate, 0.3f, "Retry Check Update");
    }

    private static void CheckForUpdate()
    {
        ResolutionManager.SetResolution(1920, 1080, Screen.fullScreen);
        isChecked = false;
        ModUpdater.DeleteOldFiles();

        foreach (var url in GetInfoFileUrlList(true))
        {
            if (!GetVersionInfo(url + "fs_info.json").GetAwaiter().GetResult()) continue;
            isChecked = true;
            break;
        }

        Msg("Check For Update: " + isChecked, "CheckRelease");
        isBroken = !isChecked;
        if (isChecked)
        {
            Info("Has Update: " + hasUpdate, "CheckRelease");
            Info("Latest Version: " + latestVersion, "CheckRelease");
            Info("Minimum Version: " + minimumVersion, "CheckRelease");
            Info("Creation: " + creation, "CheckRelease");
            Info("Force Update: " + forceUpdate, "CheckRelease");
            Info("File MD5: " + md5, "CheckRelease");
            Info("Github Url: " + downloadUrl_github, "CheckRelease");
            Info("Gitee Url: " + downloadUrl_gitee, "CheckRelease");
            //Info("Website Url: " + downloadUrl_xtremeapi, "CheckRelease");

            if (firstLaunch || isBroken)
            {
                firstLaunch = false;
                var annos = ModUpdater.announcement[TranslationController.Instance.currentLanguage.languageID];
                if (isBroken) CustomPopup.Show(GetString(StringNames.AnnouncementLabel), annos,
                    [(GetString(StringNames.ExitGame), Application.Quit)]);
                else CustomPopup.Show(GetString(StringNames.AnnouncementLabel), annos,
                    [(GetString(StringNames.Okay), null)]);
            }
        }
        else
        {
            if (retried >= 2) CustomPopup.Show(GetString("updateCheckPopupTitle"), GetString("updateCheckFailedExit"),
                [(GetString(StringNames.Okay), null)]);
            else CustomPopup.Show(GetString("updateCheckPopupTitle"), GetString("updateCheckFailedRetry"),
                [(GetString("Retry"), Retry)]);
        }
        ModUpdater.SetUpdateButtonStatus();
    }

    private static async Task<bool> GetVersionInfo(string url)
    {
        Msg(url, "CheckRelease");
        try
        {
            string result;
            if (url.StartsWith("file:///"))
            {
                result = await File.ReadAllTextAsync(url[8..]);
            }
            else
            {
                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("User-Agent", "FinalSuspect Updater");
                client.DefaultRequestHeaders.Add("Referer", "gitee.com");
                using var response = await client.GetAsync(new Uri(url), HttpCompletionOption.ResponseContentRead);
                if (!response.IsSuccessStatusCode)
                {
                    Error($"Failed: {response.StatusCode}", "CheckRelease");
                    return false;
                }
                
                result = await response.Content.ReadAsStringAsync();
                result = result.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
            }

            var data = JObject.Parse(result);

            verHead = new(data["verHead"]?.ToString());

            DebugVer = new(data["DebugVer"]?.ToString() ?? string.Empty);

            CanUpdate = bool.Parse(new(data["CanUpdate"]?.ToString()));

            verDate = new(data["verDate"]?.ToString());
            md5 = data["md5"]?.ToString();
            latestVersion = new(data["version"]?.ToString() ?? string.Empty);

            showVer = $"{verHead}_{verDate}";

            var minVer = data["minVer"]?.ToString();
            if (minVer != null) minimumVersion = minVer?.ToLower() == "latest" ? latestVersion : new(minVer);
            creation = int.Parse(data["creation"]?.ToString() ?? string.Empty);
            isBroken = data["allowStart"]?.ToString().ToLower() != "true";

            var announcement = data["announcement"].Cast<JObject>();
            foreach (var langid in EnumHelper.GetAllValues<SupportedLangs>())
            ModUpdater.announcement[langid] = announcement[langid.ToString()]?.ToString();
            downloadUrl_gitee = downloadUrl_gitee.Replace("{showVer}", showVer);
            hasUpdate = Main.version < latestVersion && creation > Main.PluginCreation;
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