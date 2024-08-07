using HarmonyLib;
using Newtonsoft.Json.Linq;
using Sentry.Unity.NativeUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using FinalSuspect_Xtreme.Modules;
using UnityEngine;
using static FinalSuspect_Xtreme.Translator;

namespace FinalSuspect_Xtreme;

[HarmonyPatch]
public class ModUpdater
{
    public static string DownloadFileTempPath = "BepInEx/plugins/FinalSuspect_Xtreme.dll.temp";
    private static IReadOnlyList<string> URLs => new List<string>
    {
#if DEBUG
        $"file:///{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "fs_info.json")}",
#else
        "https://raw.githubusercontent.com/XtremeWave/FinalSuspect_Xtreme/FinalSus/fs_info.json",
        "https://gitee.com/XtremeWave/FinalSuspect_Xtreme/raw/FinalSus/fs_info.json",
         

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
    public static int visit => isChecked ? 216822 : 0;

    public static string announcement_pre = "";
    public static string announcement_zh = "";
    public static string announcement_en = "";
    public static string downloadUrl_github = "";
    public static string downloadUrl_gitee = "";
    public static string downloadUrl_website = "";
    public static string downloadUrl_website2 = "";
    private static int retried = 0;
    private static bool firstLaunch = true;
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPostfix, HarmonyPriority(Priority.LowerThanNormal)]
    public static void StartPostfix()
    {

        CustomPopup.Init();

        if (!isChecked && firstStart) CheckForUpdate();
        SetUpdateButtonStatus();
        //if (File.Exists(@$"{Environment.CurrentDirectory.Replace(@"\", "/")}./FinalSuspect_Data/Sounds/Birthday.wav"))
        //{
        //    CustomSoundsManager.Play("Birthday", 0);
        //}
        firstStart = false;
        //CreateUIElements.Instance.Initialize();
        //CreateUIElements.Instance.Load();
    }
    public static void SetUpdateButtonStatus()
    {
        MainMenuManagerPatch.UpdateButton.SetActive(isChecked && hasUpdate && (firstStart || forceUpdate));
        MainMenuManagerPatch.PlayButton.SetActive(!MainMenuManagerPatch.UpdateButton.activeSelf);
        var buttonText = MainMenuManagerPatch.UpdateButton.transform.FindChild("FontPlacer").GetChild(0).GetComponent<TextMeshPro>();
        Logger.Info(showVer, "ver");
        buttonText.text = $"{(CanUpdate? GetString("updateButton"): GetString("updateNotice"))}\nv{showVer?.ToString() ?? " ???"}";
        Logger.Info(showVer.ToString(), "ver");
    }
    public static void Retry()
    {
        retried++;
        CustomPopup.Show(GetString("updateCheckPopupTitle"), GetString("PleaseWait"), null);
        _ = new LateTask(CheckForUpdate, 0.3f, "Retry Check Update");
    }
    public static void CheckForUpdate()
    {
        isChecked = false;
        DeleteOldFiles();

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
            Logger.Info("Github Url: " + downloadUrl_github, "CheckRelease");
            Logger.Info("Gitee Url: " + downloadUrl_gitee, "CheckRelease");
            Logger.Info("Wensite Url: " + downloadUrl_website, "CheckRelease");
            Logger.Info("Wensite2 Url: " + downloadUrl_website2, "CheckRelease");
            Logger.Info("Announcement (English): " + announcement_en, "CheckRelease");
            Logger.Info("Announcement (SChinese): " + announcement_zh, "CheckRelease");

            if (firstLaunch || isBroken)
            {
                firstLaunch = false;
                var annos = IsChineseUser ? announcement_zh : announcement_en;
                if (isBroken) CustomPopup.Show(GetString(StringNames.AnnouncementLabel), annos, new() { (GetString(StringNames.ExitGame), Application.Quit) });
                else CustomPopup.Show(GetString(StringNames.AnnouncementLabel), annos, new() { (GetString(StringNames.Okay), null) });
            }
        }
        else
        {
            if (retried >= 2) CustomPopup.Show(GetString("updateCheckPopupTitle"), GetString("updateCheckFailedExit"), new() { (GetString(StringNames.Okay), null) });
            else CustomPopup.Show(GetString("updateCheckPopupTitle"), GetString("updateCheckFailedRetry"), new() { (GetString("Retry"), Retry) });
        }

        SetUpdateButtonStatus();
    }
    public static void BeforeCheck()
    {
        isChecked = false; 
        DeleteOldFiles();
        Logger.Info("1", "gctest");
        foreach (var url in GetInfoFileUrlList())
        {
            Logger.Info($"1.5-{url}", "gctest");
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
            Logger.Info("Github Url: " + downloadUrl_github, "CheckRelease");
            Logger.Info("Gitee Url: " + downloadUrl_gitee, "CheckRelease");
            Logger.Info("Wensite Url: " + downloadUrl_website, "CheckRelease");
            Logger.Info("Wensite2 Url: " + downloadUrl_website2, "CheckRelease");
            Logger.Info("Announcement (English): " + announcement_en, "CheckRelease");
            Logger.Info("Announcement (SChinese): " + announcement_zh, "CheckRelease");
        }
    }
    public static string Get(string url)
    {
        string result = string.Empty;
        HttpClient req = new HttpClient();
        var res = req.GetAsync(url).Result;
        Stream stream = res.Content.ReadAsStreamAsync().Result;
        try
        {
            //获取内容
            using StreamReader reader = new(stream);
            result = reader.ReadToEnd();
        }
        finally
        {
            stream.Close();
        }
        return result;
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
                client.DefaultRequestHeaders.Add("User-Agent", "FinalSuspect_Xtreme Updater");
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
            announcement_en = announcement["English"]?.ToString();
            announcement_zh = announcement["SChinese"]?.ToString();

            JObject downloadUrl = data["url"].Cast<JObject>();
            downloadUrl_github = downloadUrl["github"]?.ToString();
            downloadUrl_gitee = downloadUrl["gitee"]?.ToString().Replace("{{showVer}}", $"v{showVer}");
            downloadUrl_website = downloadUrl["website"]?.ToString();
            downloadUrl_website2 = downloadUrl["website2"]?.ToString();

            MusicDownloader.Url_github = downloadUrl["githubmus"]?.ToString();
            MusicDownloader.Url_gitee = downloadUrl["giteemus"]?.ToString();
            MusicDownloader.Url_website = downloadUrl["websitemus"]?.ToString();
            MusicDownloader.Url_website2 = downloadUrl["website2mus"]?.ToString();

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
    public static void StartUpdate(string url = "waitToSelect")
    {
        if (url == "waitToSelect")
        {
            CustomPopup.Show(GetString("updatePopupTitle"), GetString("updateChoseSource"), new()
            {
                (GetString("updateSource.Website"), () => StartUpdate(downloadUrl_website)),
                (GetString("updateSource.Github"), () => StartUpdate(downloadUrl_github)),
                (GetString("updateSource.Gitee"), () => StartUpdate(downloadUrl_gitee)),
                (GetString(StringNames.Cancel), SetUpdateButtonStatus)
            });
            return;
        }

        Regex r = new Regex(@"^(http|https|ftp)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&%\$\-]+)*@)?((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.[a-zA-Z]{2,4})(\:[0-9]+)?(/[^/][a-zA-Z0-9\.\,\?\'\\/\+&%\$#\=~_\-@]*)*$");
        if (!r.IsMatch(url))
        {
            CustomPopup.ShowLater(GetString("updatePopupTitleFialed"), string.Format(GetString("updatePingFialed"), "404 Not Found"), new() { (GetString(StringNames.Okay), SetUpdateButtonStatus) });
            return;
        }

        CustomPopup.Show(GetString("updatePopupTitle"), GetString("updatePleaseWait"), null);

        var task = DownloadDLL(url);
        task.ContinueWith(t =>
        {
            var (done, reason) = t.Result;
            string title = done ? GetString("updatePopupTitleDone") : GetString("updatePopupTitleFialed");
            string desc = done ? GetString("updateRestart") : reason;
            CustomPopup.ShowLater(title, desc, new() { (GetString(done ? StringNames.ExitGame : StringNames.Okay), done ? Application.Quit : null) });
            SetUpdateButtonStatus();
        });
    }
    public static void DeleteOldFiles()
    {
        try
        {
            foreach (var path in Directory.EnumerateFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.*"))
            {
                if (path.EndsWith(Path.GetFileName(Assembly.GetExecutingAssembly().Location))) continue;
                if (path.EndsWith("FinalSuspect_Xtreme.dll") || path.EndsWith("Downloader.dll")) continue;
                Logger.Info($"{Path.GetFileName(path)} Deleted", "DeleteOldFiles");
                File.Delete(path);
            }
        }
        catch (Exception e)
        {
            Logger.Error($"清除更新残留失败\n{e}", "DeleteOldFiles");
        }
        return;
    }
    public static async Task<(bool, string)> DownloadDLL(string url)
    {
        Retry:
        File.Delete(DownloadFileTempPath);
        File.Create(DownloadFileTempPath).Close();

        Logger.Msg("Start Downlaod From: " + url, "DownloadDLL");
        Logger.Msg("Save To: " + DownloadFileTempPath, "DownloadDLL");
        try
        {
            using var client = new HttpClientDownloadWithProgress(url, DownloadFileTempPath);
            client.ProgressChanged += OnDownloadProgressChanged;
            await client.StartDownload();
            Logger.Info(GetMD5HashFromFile(DownloadFileTempPath), "md5");
            Logger.Info(md5, "md5");
            Thread.Sleep(100);
            if (GetMD5HashFromFile(DownloadFileTempPath) != md5)
            {
                File.Delete(DownloadFileTempPath);
                if (url == downloadUrl_website)
                {
                    url = downloadUrl_website2;
                    goto Retry;
                }    
                return (false, GetString("updateFileMd5Incorrect"));
            }
            else
            {
                var fileName = Assembly.GetExecutingAssembly().Location;
                File.Move(fileName, fileName + ".bak");
                File.Move("BepInEx/plugins/FinalSuspect_Xtreme.dll.temp", fileName);
                return (true, null);
            }
        }
        catch (Exception ex)
        {
            File.Delete(DownloadFileTempPath);
            Logger.Error($"更新失败\n{ex.Message}", "DownloadDLL", false);
            if (url ==downloadUrl_website)
            {
                url = downloadUrl_website2;
                goto Retry;
            }
            return (false, GetString("downloadFailed"));
        }
    }
    private static void OnDownloadProgressChanged(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage)
    {
        string msg = $"{GetString("updateInProgress")}\n{totalFileSize / 1000}KB / {totalBytesDownloaded / 1000}KB  -  {(int)progressPercentage}%";
        Logger.Info(msg, "DownloadDLL");
        CustomPopup.UpdateTextLater(msg);
    }
    public static string GetMD5HashFromFile(string fileName)
    {
        try
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(fileName);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, "GetMD5HashFromFile");
            return "";
        }
    }
}