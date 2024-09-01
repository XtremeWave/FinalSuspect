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

namespace FinalSuspect_Xtreme.Modules.CheckAndDownload;

[HarmonyPatch]
public class MusicDownloader
{
    public static string DownloadFileTempPath = "FinalSuspect_Data/Sounds/{{sound}}.wav";

    public static string downloadUrl_github = "";
    public static string downloadUrl_gitee = ""; 
    public static string downloadUrl_objectstorage = "";
    public static string downloadUrl_aumodsite = "";

    public static async Task<bool> StartDownload(string sound, string url = "waitToSelect")
    {
        retry:
        if (!Directory.Exists(@"./FinalSuspect_Data/Sounds"))
        {
            Directory.CreateDirectory(@"./FinalSuspect_Data/Sounds");
        }
        var DownloadFileTempPath = "FinalSuspect_Data/Sounds/{{sound}}.wav";
        
        var downloaddownloadUrl_github = downloadUrl_github.Replace("{{sound}}", $"{sound}");
        var downloaddownloadUrl_gitee = downloadUrl_gitee.Replace("{{sound}}", $"{sound}");
        var downloaddownloadUrl_objectstorage = downloadUrl_objectstorage.Replace("{{sound}}", $"{sound}");
        if (url == "waitToSelect")
            url = IsChineseLanguageUser ? downloaddownloadUrl_objectstorage : downloaddownloadUrl_github;

        if (!IsValidUrl(url))
        {
            Logger.Error($"Invalid URL: {url}", "DownloadSound", false);
            return false;
        }
        DownloadFileTempPath = DownloadFileTempPath.Replace("{{sound}}", $"{sound}");
        string filePath = DownloadFileTempPath + ".xwmus";
        File.Create(filePath).Close();
       

        Logger.Msg("Start Downloading from: " + url, "DownloadSound");
        Logger.Msg("Saving file to: " + DownloadFileTempPath, "DownloadSound");

        try
        {
           
            
            using var client = new HttpClientDownloadWithProgress(url, filePath);
            client.ProgressChanged += OnDownloadProgressChanged;
            await client.StartDownload();
            Thread.Sleep(100);
            if (
                !md5ForFiles.ContainsKey(sound)
                || GetMD5HashFromFile(filePath).ToLower() != md5ForFiles[sound].ToLower()
                || !File.Exists(filePath))
            {
                Logger.Error($"Md5 Wrong in {url}", "DownloadSound");
                File.Delete(filePath);
                if (url == downloaddownloadUrl_objectstorage && IsChineseLanguageUser || url == downloaddownloadUrl_github && !IsChineseLanguageUser)
                {

                    url = downloaddownloadUrl_gitee;
                    goto retry;
                }
                else if (url == downloaddownloadUrl_gitee && IsChineseLanguageUser)
                {
                    url = downloaddownloadUrl_github;
                    goto retry;
                }
                else 
                if (!string.IsNullOrEmpty(filePath))
                    File.Delete(filePath);
            }
            File.Move(filePath, DownloadFileTempPath);
            Logger.Info($"Succeed in {url}", "DownloadSound");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to download\n{ex.Message}", "DownloadSound", false);
            if (!string.IsNullOrEmpty(filePath))
            {
                File.Delete(filePath);
            }
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