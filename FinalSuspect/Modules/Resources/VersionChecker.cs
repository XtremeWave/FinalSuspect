using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FinalSuspect.ClientActions.FeatureItems.NameTag;
using FinalSuspect.ClientActions.FeatureItems.Resources;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Features;
using FinalSuspect.Modules.Features.CheckingandBlocking;
using FinalSuspect.Patches.System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace FinalSuspect.Modules.Resources;

public static class VersionChecker
{
    public static bool firstStart = true;

    public static bool hasUpdate;
    public static bool forceUpdate;
    public static bool isBroken;
    public static bool isChecked;
    public static string versionInfoRaw = "";

    private static Version latestVersion;
    public static string showVer = "";
    public static bool CanUpdate;
    private static string verHead = "";
    private static string verDate = "";
    private static Version minimumVersion;
    private static int creation;
    public static string md5 = "";

    private static int retried;
    private static bool firstLaunch = true;
    public static bool IsSupported { get; private set; } = true;

    private static async void StartTasks()
    {
        try
        {
            _ = ModNewsHistory.LoadModAnnouncements();

            await SpamManager.Init();
            await Task.Delay(100);
            await ResourcesManager.CheckForResources();
            await CheckForUpdate();
        }
        catch
        {
            /* ignored */
        }
    }

    public static void Check()
    {
        var amongUsVersion = Version.Parse(Application.version);
        var lowestSupportedVersion = Version.Parse(Main.LowestSupportedVersion);
        IsSupported = amongUsVersion >= lowestSupportedVersion;
        if (!IsSupported) ErrorText.Instance.AddError(ErrorCode.UnsupportedVersion);
    }

    private static void Retry()
    {
        retried++;
        CustomPopup.Show(GetString("UpdateCheck.Popup_Title"), GetString("Tip.PleaseWait"), null);
        _ = new LateTask(() => _ = CheckForUpdate(), 0.3f, "Retry Check Update");
    }

    private static async Task CheckForUpdate()
    {
        isChecked = false;
        ModUpdater.DeleteOldFiles();

        foreach (var url in GetInfoFileUrlList(true))
        {
            var task = GetVersionInfo(url + "fs_info.json");
            await task;
            if (!task.Result) continue;
            isChecked = true;
            break;
        }

        _ = new MainThreadTask(() =>
        {
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

                if (firstLaunch || isBroken)
                {
                    firstLaunch = false;
                    var annos = ModUpdater.announcement[TranslationController.Instance.currentLanguage.languageID];
                    if (isBroken)
                        CustomPopup.Show(GetString(StringNames.AnnouncementLabel), annos,
                            [(GetString(StringNames.ExitGame), Application.Quit)]);
                    else
                        CustomPopup.Show(GetString(StringNames.AnnouncementLabel), annos,
                            [(GetString(StringNames.Okay), null)]);
                }
            }
            else
            {
                if (retried >= 2)
                    CustomPopup.Show(GetString("UpdateCheck.Popup_Title"), GetString("UpdateCheck.Failed_Exit"),
                        [(GetString(StringNames.Okay), null)]);
                else
                    CustomPopup.Show(GetString("UpdateCheck.Popup_Title"), GetString("UpdateCheck.Failed_Retry"),
                        [(GetString("Retry"), Retry)]);
            }

            ModUpdater.SetUpdateButtonStatus();
            VersionShowerStartPatch.VisitText.text = isChecked
                ? string.Format(GetString("FinalSuspectWelcomeText"), ColorHelper.FSColorHex)
                : GetString("RetrieveVersionInfoFailed");
        }, "Check For Update");
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

            verHead = new string(data["verHead"]?.ToString());

            CanUpdate = bool.Parse(new string(data["CanUpdate"]?.ToString()));

            verDate = new string(data["verDate"]?.ToString());
            md5 = data["md5"]?.ToString();
            latestVersion = new Version(data["version"]?.ToString() ?? string.Empty);

            showVer = $"{verHead}_{verDate}";

            var minVer = data["minVer"]?.ToString();
            if (minVer != null) minimumVersion = minVer.ToLower() == "latest" ? latestVersion : new Version(minVer);
            creation = int.Parse(data["creation"]?.ToString() ?? string.Empty);
            isBroken = data["allowStart"]?.ToString().ToLower() != "true";

            var announcement = data["announcement"].Cast<JObject>();
            foreach (var langid in EnumHelper.GetAllValues<SupportedLangs>())
                ModUpdater.announcement[langid] = announcement[langid.ToString()]?.ToString();
            downloadUrl_gitee = downloadUrl_gitee.Replace("{showVer}", showVer);
            hasUpdate = Main.version < latestVersion && creation > Main.PluginCreation;
            forceUpdate = Main.version < minimumVersion || creation > Main.PluginCreation;

            return true;
        }
        catch
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    [HarmonyPriority(Priority.LowerThanNormal)]
    public class Start
    {
        public static void Postfix()
        {
            CustomPopup.Init();
            if (firstStart)
            {
                StartTasks();
                CustomPopup.Show(GetString("UpdateCheck.Popup_Title"), GetString("Tip.LoadingWithDot"), null);
            }

            NameTagManager.ReloadTag(null);
            ModUpdater.SetUpdateButtonStatus();
            firstStart = false;
        }
    }
}