using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FinalSuspect.Modules.Resources;

public static class ResourcesDownloader
{
    public static async Task<bool> StartDownload(FileType fileType, string file)
    {
        string filePath;
        switch (fileType)
        {
            case FileType.Images:
            case FileType.Musics:
            case FileType.ModNews:
            case FileType.Languages:
            case FileType.SoundEffects:
                filePath = GetResourceFilesPath(fileType, file);
                break;
            case FileType.Depends:
                filePath = GetLocalPath(LocalType.BepInEx) + file;
                break;
            case FileType.Unknown:
            default:
                return false;
        }

        var downloadFileTempPath = filePath + ".xwr";

        var retryTimes = IsChineseLanguageUser ? 0 : 3;
        retry:
        var remoteType = (RemoteType)retryTimes;

        var url = GetFile(fileType, remoteType, file);

        if (!IsValidUrl(url))
        {
            Error($"Invalid URL: {url}", "Download Resources", false);
            return false;
        }

        File.Create(downloadFileTempPath).Close();

        Msg("Start Downloading from: " + url, "Download Resources");
        Msg("Saving file to: " + filePath, "Download Resources");

        try
        {
            using var client = new HttpClientDownloadWithProgress(url, downloadFileTempPath);
            await client.StartDownload();
            Thread.Sleep(100);
            File.Delete(filePath);
            File.Move(downloadFileTempPath, filePath);

            if (Path.GetExtension(filePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var extractPath = Path.GetDirectoryName(filePath);
                    Msg($"Unzipping file: {filePath}", "Download Resources");
                    if (extractPath != null) ZipFile.ExtractToDirectory(filePath, extractPath);
                    File.Delete(filePath);
                    Warn($"Unzipped successfully: {filePath}", "Download Resources");
                }
                catch (Exception ex)
                {
                    Error($"Failed to unzip file\n{ex.Message}", "Download Resources", false);
                    return false;
                }
            }

            Warn($"Succeed in {url}", "Download Resources");
            return true;
        }
        catch (Exception ex)
        {
            Error($"Failed to download\n{ex.Message}", "Download Resources", false);
            File.Delete(downloadFileTempPath);
            retryTimes++;
            if (retryTimes < 4)
                goto retry;
            return false;
        }
    }

    public static async Task<bool> StartDownloadAsPackage(string packageName, FileType fileType, string file)
    {
        string filePath;
        switch (fileType)
        {
            case FileType.Images:
            case FileType.ModNews:
            case FileType.Languages:
            case FileType.Musics:
            case FileType.SoundEffects:
                filePath = GetResourceFilesPath(fileType, file);
                break;
            case FileType.Depends:
                filePath = GetLocalPath(LocalType.BepInEx) + file;
                break;
            case FileType.Unknown:
            default:
                return false;
        }

        var DownloadFileTempPath = filePath + ".xwr";

        var retrytimes = 0;
        var remoteType = RemoteType.Github;
        retry:
        if (IsChineseLanguageUser)
            remoteType = (RemoteType)retrytimes;

        var url = GetPackageFile(packageName, remoteType, file);

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
            File.Delete(filePath);
            File.Move(DownloadFileTempPath, filePath);
            Warn($"Succeed in {url}", "Download Resources");
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
        const string pattern = @"^(https?|ftp)://[^\s/$.?#].[^\s]*$";
        return Regex.IsMatch(url, pattern);
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

    /*private static void OnDownloadProgressChanged(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage)
    {
        var msg = $"\n{totalFileSize / 1000}KB / {totalBytesDownloaded / 1000}KB  -  {(int)progressPercentage}%";
        Info(msg, "Download Resources");
    }

    public static async Task<bool> IsUrl404Async(FileType fileType, string file)
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
                var urlApi = PathManager.GetFile(fileType, RemoteType.FinalApi, file);
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