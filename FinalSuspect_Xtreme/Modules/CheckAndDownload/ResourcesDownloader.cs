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
using static FinalSuspect_Xtreme.Translator;
using static FinalSuspect_Xtreme.Modules.CheckAndDownload.VersionChecker;
using UnityEngine;

namespace FinalSuspect_Xtreme.Modules.CheckAndDownload;

public class ResourcesDownloader
{
    public static string ImagesSavePath = "FinalSuspect_Data/Resources/Images/";
    public static string DependsSavePath = "BepInEx/core/";

    public static readonly string ImagedownloadUrl_github = GithubUrl + "raw/FinalSuspect_Xtreme/Assets/";
    public static readonly string ImagedownloadUrl_gitee = GiteeUrl + "raw/FinalSuspect_Xtreme/Assets/";
    public static readonly string ImagedownloadUrl_objectstorage = ObjectStorageUrl;
    public static readonly string ImagedownloadUrl_aumodsite = AUModSiteUrl;

    public static readonly string DependsdownloadUrl_github = GithubUrl + "raw/FinalSuspect_Xtreme/Assets/";
    public static readonly string DependsdownloadUrl_gitee = GiteeUrl + "raw/FinalSuspect_Xtreme/Assets/";
    public static readonly string DependsdownloadUrl_objectstorage = ObjectStorageUrl;
    public static readonly string DependsedownloadUrl_aumodsite = AUModSiteUrl;

    public static System.Collections.IEnumerable CheckForFiles()
    {
        
        yield break;
    }

    public static async Task<bool> StartDownload(string resourcepath, string localpath)
    {
        if (!Directory.Exists(ImagesSavePath))
        Directory.CreateDirectory(ImagesSavePath);
        if (!Directory.Exists(DependsSavePath))
            Directory.CreateDirectory(DependsSavePath);


        string DownloadImageFileTempPath = resourcepath + ".xwr";

        if (!IsValidUrl(resourcepath))
        {
            Logger.Error($"Invalid URL: {resourcepath}", "Download Resources", false);
            return false;
        }

        File.Create(localpath).Close();
       

        Logger.Msg("Start Downloading from: " + resourcepath, "DownloadSound");
        Logger.Msg("Saving file to: " + localpath, "DownloadSound");

        try
        {
            using var client = new HttpClientDownloadWithProgress(resourcepath, DownloadImageFileTempPath);
            client.ProgressChanged += OnDownloadProgressChanged;
            await client.StartDownload();
            Thread.Sleep(100);
            Logger.Info($"Succeed in {resourcepath}", "DownloadSound");
            File.Move(DownloadImageFileTempPath, localpath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to download\n{ex.Message}", "DownloadSound", false);
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
        Logger.Info(msg , "Download Resouces");
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