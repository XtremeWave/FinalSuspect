using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FinalSuspect.Modules.Resources;

public class ResourcesDownloader
{
    
    public static async Task<bool> StartDownload(FileType fileType, string file)
    {
        string filePath;
        switch (fileType)
        {
            case FileType.Images:
            case FileType.Sounds:
            case FileType.ModNews:
            case FileType.Languages:
                filePath = PathManager.GetResourceFilesPath(fileType, file);
                break;
            case FileType.Depends:
                filePath = PathManager.GetLocalPath(LocalType.BepInEx) +file;
                break;
            default:
                return false;
        }
        string DownloadFileTempPath = filePath + ".xwr";

        var retrytimes = 0;
        RemoteType remoteType = RemoteType.Github;
        
    retry:
        if (IsChineseLanguageUser)
            switch (retrytimes)
            {
                case 0:
                    remoteType = RemoteType.XtremeApi;
                    break;
                case 1:
                    remoteType = RemoteType.Gitee;
                    break;

            }

        var url = PathManager.GetFile(fileType, remoteType, file);


        if (!IsValidUrl(url))
        {
            XtremeLogger.Error($"Invalid URL: {url}", "Download Resources", false);
            return false;
        }

        File.Create(DownloadFileTempPath).Close();
        
        XtremeLogger.Msg("Start Downloading from: " + url, "Download Resources");
        XtremeLogger.Msg("Saving file to: " + filePath, "Download Resources");

        try
        {
            using var client = new HttpClientDownloadWithProgress(url, DownloadFileTempPath);
            await client.StartDownload();
            Thread.Sleep(100);
            XtremeLogger.Info($"Succeed in {url}", "Download Resources");
            File.Delete(filePath);
            File.Move(DownloadFileTempPath, filePath);
            return true;
        }
        catch (Exception ex)
        {
            XtremeLogger.Error($"Failed to download\n{ex.Message}", "Download Resources", false);
            File.Delete(DownloadFileTempPath);
            retrytimes++;
            if (retrytimes < 2) 
                goto retry;
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
        XtremeLogger.Info(msg, "Download Resources");
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
            XtremeLogger.Exception(ex, "GetMD5HashFromFile");
            return "";
        }
    }
}