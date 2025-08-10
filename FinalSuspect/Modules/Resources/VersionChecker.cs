using System;
using System.Threading;
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
    public static bool FirstStart = true;

    public static bool HasUpdate;
    public static bool ForceUpdate;
    public static bool IsBroken;
    public static bool IsChecked;

    private static Version _latestVersion;
    public static string ShowVer = "";
    public static bool CanUpdate;
    private static string _verHead = "";
    private static string _verDate = "";
    private static Version _minimumVersion;
    private static int _creation;
    public static string MD5 = "";

    private static int _retried;
    private static bool _firstLaunch = true;

    public static readonly CancellationTokenSource CancellationToken = new();
    public static bool IsSupported { get; private set; } = true;

    private static async void StartTasks()
    {
        try
        {
            _ = ModNewsHistory.LoadModAnnouncements(CancellationToken.Token);
            await Task.Delay(100);
            await SpamManager.Init();
            await Task.Delay(100);
            await ResourcesManager.CheckForResources();
            await Task.Delay(100);
            await CheckForUpdate();
        }
        catch
        {
            /* ignored */
        }
    }

    private static void Check()
    {
        var amongUsVersion = Version.Parse(Application.version);
        var lowestSupportedVersion = Version.Parse(Main.LowestSupportedVersion);
        IsSupported = amongUsVersion >= lowestSupportedVersion;
        if (!IsSupported) ErrorText.Instance.AddError(ErrorCode.UnsupportedVersion);
    }

    private static void Retry()
    {
        _retried++;
        CustomPopup.Show(GetString("UpdateCheck.Popup_Title"), GetString("Tip.PleaseWait"), null);
        _ = new LateTask(() => _ = CheckForUpdate(), 0.3f, "Retry Check Update");
    }

    private static async Task CheckForUpdate()
    {
        IsChecked = false;
        ModUpdater.DeleteOldFiles();

        foreach (var url in GetInfoFileUrlList(true))
        {
            var task = GetVersionInfo(url + "fs_info.json");
            await task;
            if (!task.Result) continue;
            IsChecked = true;
            break;
        }

        _ = new MainThreadTask(() =>
        {
            Msg("Check For Update: " + IsChecked, "CheckRelease");
            IsBroken = !IsChecked;
            if (IsChecked)
            {
                Info("Has Update: " + HasUpdate, "CheckRelease");
                Info("Latest Version: " + _latestVersion, "CheckRelease");
                Info("Minimum Version: " + _minimumVersion, "CheckRelease");
                Info("Creation: " + _creation, "CheckRelease");
                Info("Force Update: " + ForceUpdate, "CheckRelease");
                Info("File MD5: " + MD5, "CheckRelease");

                if (_firstLaunch || IsBroken)
                {
                    _firstLaunch = false;
                    var annos = ModUpdater.announcement[TranslationController.Instance.currentLanguage.languageID];
                    if (IsBroken)
                        CustomPopup.Show(GetString(StringNames.AnnouncementLabel), annos,
                            [(GetString(StringNames.ExitGame), Application.Quit)]);
                    else
                        CustomPopup.Show(GetString(StringNames.AnnouncementLabel), annos,
                            [(GetString(StringNames.Okay), null)]);
                }
            }
            else
            {
                if (_retried >= 2)
                    CustomPopup.Show(GetString("UpdateCheck.Popup_Title"), GetString("UpdateCheck.Failed_Exit"),
                        [(GetString(StringNames.Okay), null)]);
                else
                    CustomPopup.Show(GetString("UpdateCheck.Popup_Title"), GetString("UpdateCheck.Failed_Retry"),
                        [(GetString("Retry"), Retry)]);
            }

            ModUpdater.SetUpdateButtonStatus();
            VersionShowerStartPatch.VisitText.text = IsChecked
                ? string.Format(GetString("FinalSuspectWelcomeText"), ColorHelper.FSColorHex)
                : GetString("RetrieveVersionInfoFailed");
        }, "Check For Update");
    }

    private static async Task<bool> GetVersionInfo(string url)
    {
        Msg(url, "CheckRelease");
        try
        {
            var task = RemoteHelper.GetRemoteStringAsync(url);
            await task;
            var (result, succeed) = task.Result;
            if (!succeed) return false;

            var data = JObject.Parse(result);

            _verHead = new string(data["verHead"]?.ToString());

            CanUpdate = bool.Parse(new string(data["CanUpdate"]?.ToString()));

            _verDate = new string(data["verDate"]?.ToString());
            MD5 = data["md5"]?.ToString();
            _latestVersion = new Version(data["version"]?.ToString() ?? string.Empty);

            ShowVer = $"{_verHead}_{_verDate}";

            var minVer = data["minVer"]?.ToString();
            if (minVer != null) _minimumVersion = minVer.ToLower() == "latest" ? _latestVersion : new Version(minVer);
            _creation = int.Parse(data["creation"]?.ToString() ?? string.Empty);
            IsBroken = data["allowStart"]?.ToString().ToLower() != "true";

            var announcement = data["announcement"].Cast<JObject>();
            foreach (var langid in EnumHelper.GetAllValues<SupportedLangs>())
                ModUpdater.announcement[langid] = announcement[langid.ToString()]?.ToString();
            DownloadUrl_Gitee = DownloadUrl_Gitee.Replace("{showVer}", ShowVer);
            HasUpdate = Main.version < _latestVersion && _creation > Main.PluginCreation;
            ForceUpdate = Main.version < _minimumVersion || _creation > Main.PluginCreation;

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
            if (FirstStart && !Main.OfflineMode.Value)
            {
                Check();
                StartTasks();
            }

            if (!IsChecked)
                CustomPopup.Show(GetString("UpdateCheck.Popup_Title"), GetString("Tip.LoadingWithDot"), null);
            NameTagManager.ReloadTag(null);
            ModUpdater.SetUpdateButtonStatus();
            FirstStart = false;
        }
    }
}