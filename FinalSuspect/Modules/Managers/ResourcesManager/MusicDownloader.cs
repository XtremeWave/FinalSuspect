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

namespace FinalSuspect.Modules.Managers.ResourcesManager;

[HarmonyPatch]
public class MusicDownloader
{
    public static string SavePath = "Final Suspect_Data/Resources/Audios";

    public static readonly string downloadUrl_github = GithubUrl + "raw/FinalSus/Assets/Sounds/{{sound}}.wav";
    public static readonly string downloadUrl_gitee = GiteeUrl + "raw/FinalSus/Assets/Sounds/{{sound}}.wav";
    public static readonly string downloadUrl_objectstorage = ObjectStorageUrl + "Sounds/{{sound}}.wav";
    public static readonly string downloadUrl_aumodsite = AUModSiteUrl + "Sounds/{{sound}}.wav";

    public static async Task<bool> StartDownload(string sound)
    {
        if (!Directory.Exists(SavePath))

            Directory.CreateDirectory(SavePath);

        var filePath = $"{SavePath}/{sound}.wav";
        string DownloadFileTempPath = filePath + ".xwr";

        var url = IsChineseLanguageUser ? downloadUrl_objectstorage : downloadUrl_github;

    retry:
        url = url.Replace("{{sound}}", $"{sound}");


        if (!IsValidUrl(url))
        {
            Logger.Error($"Invalid URL: {url}", "DownloadSound", false);
            return false;
        }

        File.Create(DownloadFileTempPath).Close();


        Logger.Msg("Start Downloading from: " + url, "DownloadSound");
        Logger.Msg("Saving file to: " + filePath, "DownloadSound");

        try
        {
            using var client = new HttpClientDownloadWithProgress(url, DownloadFileTempPath);
            client.ProgressChanged += OnDownloadProgressChanged;
            await client.StartDownload();
            Thread.Sleep(100);
            if (
                !md5ForFiles.ContainsKey(sound)
                || GetMD5HashFromFile(DownloadFileTempPath).ToLower() != md5ForFiles[sound].ToLower()
                || !File.Exists(DownloadFileTempPath))
            {
                Logger.Error($"Md5 Wrong in {url}", "DownloadSound");
                File.Delete(DownloadFileTempPath);
                if (url == downloadUrl_objectstorage && IsChineseLanguageUser)
                {
                    url = downloadUrl_gitee;
                    goto retry;
                }
                else if (url == downloadUrl_github && !IsChineseLanguageUser)
                {
                    url = downloadUrl_objectstorage;
                    goto retry;
                }
                else
                    File.Delete(DownloadFileTempPath);
            }

            Logger.Info($"Succeed in {url}", "DownloadSound");
            File.Move(DownloadFileTempPath, filePath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to download\n{ex.Message}", "DownloadSound", false);
            File.Delete(DownloadFileTempPath);
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
        Logger.Info(msg, "DownloadSounds");
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
    private static Dictionary<string, string> md5ForFiles = new()
    {
        //“Ù¿÷
        {"ElegyOfFracturedVow","183001938142209dd99086326db0cb30"},
        {"Fractured","8161a1939e042fe763796d4ef73f7a3b" },
        {"GongXiFaCai","db200d93e613020d62645f4841dd55bd"},
        {"Interlude","a5a1e5e78571107be7a71eae9c5228b5" },
        {"NeverGonnaGiveYouUp","354cab3103b7e033c6e31d12766eb59c" },
        {"ReturnToSimplicity","82ac19d1d1daa788b7349e39811bedf9" },
        {"TidalSurge","679c55048802ea02ad7ede76addeaf21"},
        {"TrailOfTruth","b3fc4e8313f76735ecfdea1e0221bc3b"},
        {"VestigiumSplendoris","82de05a534a5876fc53bebd7919e58e6" },
    };
}