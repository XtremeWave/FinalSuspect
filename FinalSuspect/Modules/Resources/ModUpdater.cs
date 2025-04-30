using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FinalSuspect.Modules.Features;
using FinalSuspect.Patches.System;
using TMPro;
using UnityEngine;
using static FinalSuspect.Modules.Resources.VersionChecker;

namespace FinalSuspect.Modules.Resources;

[HarmonyPatch]
public class ModUpdater
{
    public static readonly Dictionary<SupportedLangs, string> announcement = new();

    public static void SetUpdateButtonStatus()
    {
        MainMenuManagerPatch.UpdateButton.SetActive(isChecked && hasUpdate && (firstStart || forceUpdate));
        MainMenuManagerPatch.PlayButton.SetActive(!MainMenuManagerPatch.UpdateButton.activeSelf);
        var buttonText = MainMenuManagerPatch.UpdateButton.transform.FindChild("FontPlacer").GetChild(0).GetComponent<TextMeshPro>();
        buttonText.text = $"{(CanUpdate ? GetString("updateButton") : GetString("updateNotice"))}\nv{showVer ?? " ???"}";
    }
    
    public static void StartUpdate(string url = "waitToSelect")
    {
        if (url == "waitToSelect")
        {
            CustomPopup.Show(GetString("updatePopupTitle"), GetString("updateChoseSource"),
            [
                //(GetString("updateSource.XtremeApi"), () => StartUpdate(downloadUrl_xtremeapi)),
                (GetString("updateSource.Github"), () => StartUpdate(downloadUrl_github)),
                (GetString("updateSource.Gitee"), () => StartUpdate(downloadUrl_gitee)),
                (GetString(StringNames.Cancel), SetUpdateButtonStatus)
            ]);
            return;
        }

        var r = new Regex(@"^(http|https|ftp)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&%\$\-]+)*@)?((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.[a-zA-Z]{2,4})(\:[0-9]+)?(/[^/][a-zA-Z0-9\.\,\?\'\\/\+&%\$#\=~_\-@]*)*$");
        if (!r.IsMatch(url))
        {
            CustomPopup.ShowLater(GetString("updatePopupTitleFailed"), string.Format(GetString("updatePingFialed"), "404 Not Found"),
                [(GetString(StringNames.Okay), SetUpdateButtonStatus)]);
            return;
        }

        CustomPopup.Show(GetString("updatePopupTitle"), GetString("updatePleaseWait"), null);

        var task = DownloadDLL(url);
        task.ContinueWith(t =>
        {
            var (done, reason) = t.Result;
            var title = done ? GetString("updatePopupTitleDone") : GetString("updatePopupTitleFailed");
            var desc = done ? GetString("updateRestart") : reason;
            CustomPopup.ShowLater(title, desc,
                [(GetString(done ? StringNames.ExitGame : StringNames.Okay), done ? Application.Quit : null)]);
            SetUpdateButtonStatus();
        });
    }
    public static void DeleteOldFiles()
    {
        try
        {
            foreach (var path in Directory.EnumerateFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "*.*"))
            {
                if (path.EndsWith(Path.GetFileName(Assembly.GetExecutingAssembly().Location))) continue;
                if (path.EndsWith("FinalSuspect.dll") || path.EndsWith("Downloader.dll")) continue;

                Info($"{Path.GetFileName(path)} Deleted", "DeleteOldFiles");
                File.Delete(path);
            }
        }
        catch (Exception e)
        {
            Error($"清除更新残留失败\n{e}", "DeleteOldFiles");
        }
    }
    public static async Task<(bool, string)> DownloadDLL(string url)
    {
        File.Delete(DownloadFileTempPath);
        File.Create(DownloadFileTempPath).Close();

        Msg("Start Downlaod From: " + url, "DownloadDLL");
        Msg("Save To: " + DownloadFileTempPath, "DownloadDLL");
        try
        {
            using var client = new HttpClientDownloadWithProgress(url, DownloadFileTempPath);
            client.ProgressChanged += OnDownloadProgressChanged;
            await client.StartDownload();
            Thread.Sleep(100);
            if (GetMD5HashFromFile(DownloadFileTempPath) != md5)
            {
                File.Delete(DownloadFileTempPath);
                return (false, GetString("updateFileMd5Incorrect"));
            }
            var fileName = Assembly.GetExecutingAssembly().Location;
            File.Move(fileName, fileName + ".bak");
            File.Move(DownloadFileTempPath, fileName);
            return (true, null);
        }
        catch (Exception ex)
        {
            File.Delete(DownloadFileTempPath);
            Error($"更新失败\n{ex.Message}", "DownloadDLL", false);
            return (false, GetString("downloadFailed"));
        }
    }
    private static void OnDownloadProgressChanged(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage)
    {
        if (progressPercentage != null)
        {
            var msg = $"{GetString("updateInProgress")}\n{totalFileSize / 1000}KB / {totalBytesDownloaded / 1000}KB  -  {(int)progressPercentage}%";
            Info(msg, "DownloadDLL");
            CustomPopup.UpdateTextLater(msg);
        }
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
            Exception(ex, "GetMD5HashFromFile");
            return "";
        }
    }
}