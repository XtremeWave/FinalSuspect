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
using static FinalSuspect_Xtreme.Modules.CheckAndDownload.VersionChecker;

namespace FinalSuspect_Xtreme.Modules.CheckAndDownload;

[HarmonyPatch]
public class ModUpdater
{
    public static string DownloadFileTempPath = "BepInEx/plugins/FinalSuspect_Xtreme.dll.temp";

    public static string announcement_pre = "";
    public static string announcement_zh = "";
    public static string announcement_en = "";
    public static string downloadUrl_github = "";
    public static string downloadUrl_gitee = "";
    public static string downloadUrl_objectstorage = "";


    public static void SetUpdateButtonStatus()
    {
        MainMenuManagerPatch.UpdateButton.SetActive(isChecked && hasUpdate && (firstStart || forceUpdate));
        MainMenuManagerPatch.PlayButton.SetActive(!MainMenuManagerPatch.UpdateButton.activeSelf);
        var buttonText = MainMenuManagerPatch.UpdateButton.transform.FindChild("FontPlacer").GetChild(0).GetComponent<TextMeshPro>();
        buttonText.text = $"{(CanUpdate ? GetString("updateButton") : GetString("updateNotice"))}\nv{showVer?.ToString() ?? " ???"}";
    }
    public static void StartUpdate(string url = "waitToSelect")
    {
        if (url == "waitToSelect")
        {
            CustomPopup.Show(GetString("updatePopupTitle"), GetString("updateChoseSource"), new()
            {
                (GetString("updateSource.Website"), () => StartUpdate(downloadUrl_objectstorage)),
                (GetString("updateSource.Github"), () => StartUpdate(downloadUrl_github)),
                (GetString("updateSource.Gitee"), () => StartUpdate(downloadUrl_gitee)),
                (GetString(StringNames.Cancel), SetUpdateButtonStatus)
            });
            return;
        }

        var r = new Regex(@"^(http|https|ftp)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&%\$\-]+)*@)?((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.[a-zA-Z]{2,4})(\:[0-9]+)?(/[^/][a-zA-Z0-9\.\,\?\'\\/\+&%\$#\=~_\-@]*)*$");
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
    }
    public static async Task<(bool, string)> DownloadDLL(string url)
    {
        File.Delete(DownloadFileTempPath);
        File.Create(DownloadFileTempPath).Close();

        Logger.Msg("Start Downlaod From: " + url, "DownloadDLL");
        Logger.Msg("Save To: " + DownloadFileTempPath, "DownloadDLL");
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
            File.Move("BepInEx/plugins/FinalSuspect_Xtreme.dll.temp", fileName);
            return (true, null);
        }
        catch (Exception ex)
        {
            File.Delete(DownloadFileTempPath);
            Logger.Error($"更新失败\n{ex.Message}", "DownloadDLL", false);
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
