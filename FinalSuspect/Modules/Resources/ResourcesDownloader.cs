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
                filePath = GetResourceFilesPath(fileType, file);
                break;
            case FileType.Depends:
                filePath = GetLocalPath(LocalType.BepInEx) +file;
                break;
            default:
                return false;
        }
        var DownloadFileTempPath = filePath + ".xwr";

        var retrytimes = 0;
        var remoteType = RemoteType.Github; 
        retry:
        if (IsChineseLanguageUser)
            switch (retrytimes)
            {
                //case 0:
                //    remoteType = RemoteType.XtremeApi;
                //    break;
                case 1:
                    remoteType = RemoteType.Gitee;
                    break;
                case 2:
                    remoteType = RemoteType.Github;
                    break;
            }

        var url = GetFile(fileType, remoteType, file);

        if (!IsValidUrl(url))
        {
            Error($"Invalid URL: {url}", "Download Resources", false);
            return false;
        }

        File.Create(DownloadFileTempPath).Close();
        
        Msg("Start Downloading from: " + url, "Download Resources");
        Msg("Saving file to: " + filePath, "Download Resources");

        try
        {
            using var client = new HttpClientDownloadWithProgress(url, DownloadFileTempPath);
            await client.StartDownload();
            Thread.Sleep(100);
            Info($"Succeed in {url}", "Download Resources");
            File.Delete(filePath);
            File.Move(DownloadFileTempPath, filePath);
            return true;
        }
        catch (Exception ex)
        {
            Error($"Failed to download\n{ex.Message}", "Download Resources", false);
            File.Delete(DownloadFileTempPath);
            retrytimes++;
            if (retrytimes < 3) 
                goto retry;
            return false;
        }
    }
    private static bool IsValidUrl(string url)
    {
        var pattern = @"^(https?|ftp)://[^\s/$.?#].[^\s]*$";
        return Regex.IsMatch(url, pattern);
    }
    /*private static void OnDownloadProgressChanged(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage)
    {
        var msg = $"\n{totalFileSize / 1000}KB / {totalBytesDownloaded / 1000}KB  -  {(int)progressPercentage}%";
        Info(msg, "Download Resources");
    }*/
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
    /*public static async Task<bool> IsUrl404Async(FileType fileType, string file)
    {
        return false;
            using var client = new HttpClient();
            try
            {
                if (!IsChineseLanguageUser)
                {
                    var urlGithub = PathManager.GetFile(fileType, RemoteType.Github, file);

                    var response = await client.GetAsync(urlGithub);
                    return response.StatusCode == HttpStatusCode.NotFound;
                }

                var urlGitee = PathManager.GetFile(fileType, RemoteType.Gitee, file);
                var urlApi = PathManager.GetFile(fileType, RemoteType.XtremeApi, file);
                var response1 = await client.GetAsync(urlGitee);
                var response2 = await client.GetAsync(urlApi);
                return response1.StatusCode == HttpStatusCode.NotFound && response2.StatusCode == HttpStatusCode.NotFound;
            }

        catch
        {
            return false;
        }
    }*/
}