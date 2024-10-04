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
using static FinalSuspect.Translator;
using static FinalSuspect.Modules.Managers.ResourcesManager.VersionChecker;
using UnityEngine;
using FinalSuspect.Modules.Managers;

namespace FinalSuspect.Modules.Managers.ResourcesManager;

public class ResourcesDownloader
{
    public static string ImagesSavePath = "FinalSuspect_Data/Resources/Images/";
    public static string DependsSavePath = "BepInEx/core/";

    public static readonly string ImagedownloadUrl_github = GithubUrl + "raw/FinalSus/Assets/";
    public static readonly string ImagedownloadUrl_gitee = GiteeUrl + "raw/FinalSus/Assets/";
    public static readonly string ImagedownloadUrl_objectstorage = ObjectStorageUrl;
    public static readonly string ImagedownloadUrl_aumodsite = AUModSiteUrl;

    public static readonly string DependsdownloadUrl_github = GithubUrl + "raw/FinalSus/Assets/";
    public static readonly string DependsdownloadUrl_gitee = GiteeUrl + "raw/FinalSus/Assets/";
    public static readonly string DependsdownloadUrl_objectstorage = ObjectStorageUrl;
    public static readonly string DependsedownloadUrl_aumodsite = AUModSiteUrl;

    public static System.Collections.IEnumerable CheckForFiles()
    {

        yield break;
    }

    public static async Task<bool> StartDownload(string resourcepath, string localpath)
    {



        string DownloadImageFileTempPath = localpath + ".xwr";

        if (!IsValidUrl(resourcepath))
        {
            Logger.Error($"Invalid URL: {resourcepath}", "Download Resources", false);
            return false;
        }

        File.Create(DownloadImageFileTempPath).Close();
        Logger.Msg("Start Downloading from: " + resourcepath, "Download Resources");
        Logger.Msg("Saving file to: " + localpath, "Download Resources");

        try
        {
            using var client = new HttpClientDownloadWithProgress(resourcepath, DownloadImageFileTempPath);
            await client.StartDownload();
            Thread.Sleep(100);
            Logger.Info($"Succeed in {resourcepath}", "Download Resources");
            File.Move(DownloadImageFileTempPath, localpath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to download\n{ex.Message}", "Download Resources", false);
            File.Delete(DownloadImageFileTempPath);
            return false;
        }

    }
    private static bool IsValidUrl(string url)
    {
        string pattern = @"^(https?|ftp)://[^\s/$.?#].[^\s]*$";
        return Regex.IsMatch(url, pattern);
    }
    private static void OnDownloadProgressChanged(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage)
    {
        string msg = $"\n{totalFileSize / 1000}KB / {totalBytesDownloaded / 1000}KB  -  {(int)progressPercentage}%";
        Logger.Info(msg, "Download Resources");
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