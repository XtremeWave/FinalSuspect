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

namespace FinalSuspect_Xtreme.Modules.CheckAndDownload;

[HarmonyPatch]
public class MusicDownloader
{
    public static string SavePath = "FinalSuspect_Data/Resources/Audios";

    public static readonly string downloadUrl_github = GithubUrl + "raw/FinalSuspect_Xtreme/Assets/Sounds/{{sound}}.wav";
    public static readonly string downloadUrl_gitee = GiteeUrl + "raw/FinalSuspect_Xtreme/Assets/Sounds/{{sound}}.wav";
    public static readonly string downloadUrl_objectstorage = ObjectStorageUrl + "Sounds/{{sound}}.wav";
    public static readonly string downloadUrl_aumodsite = AUModSiteUrl + "Sounds/{{sound}}.wav";

    public static async Task<bool> StartDownload(string sound, string url = "waitToSelect")
    {
        if (!Directory.Exists(SavePath))
        
        Directory.CreateDirectory(SavePath);
        
        var filePath = $"{SavePath}/{sound}.wav";
        string DownloadFileTempPath = filePath + ".xwm";

    retry:

        if (url == "waitToSelect")
            url = IsChineseLanguageUser ? downloadUrl_objectstorage : downloadUrl_github;
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
            File.Move(DownloadFileTempPath, filePath);
            Logger.Info($"Succeed in {url}", "DownloadSound");
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
        Logger.Info(msg , "DownloadSounds");
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
        {"GongXiFaCaiLiuDeHua","DB200D93E613020D62645F4841DD55BD"},
        {"NeverGonnaGiveYouUp","354cab3103b7e033c6e31d12766eb59c" },
        {"RejoiceThisSEASONRespectThisWORLD","7AB4778744242E4CFA0468568308EA9B"},
        {"SpringRejoicesinParallelUniverses","D92528104A82DBBFADB4FF251929BA5E"},
        {"AFamiliarPromise", "a3672341f586b4d81efba6d4278cfeae"},
{"GuardianandDream", "cd8fb04bad5755937496eed60c4892f3"},
{"HeartGuidedbyLight", "f1ded08a59936b8e1db95067a69b006e"},
{"HopeStillExists", "8d5ba9ac283e156ab2c930f7b63a4a36"},
{"Mendax", "1054c90edfa66e31655bc7a58f553231"},
{"MendaxsTimeForExperiment", "1b82e1ea81aeb9a968a94bec7f4f62fd"},
{"StarfallIntoDarkness", "46f09e0384eb8a087c3ba8cc22e4ac11"},
{"StarsFallWithDomeCrumbles", "b5ccabeaf3324cedb107c83a2dc0ce1e"},
{"TheDomeofTruth", "183804914e3310b9f92b47392f503a9f"},
{"TheTruthFadesAway", "75fbed53db391ed73085074ad0709d82"},
{"unavoidable", "da520f4613103826b4df7647e368d4b4"},



    };
}