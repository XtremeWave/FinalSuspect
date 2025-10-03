using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FinalSuspect.ClientItems.FeatureItems.MyMusic;

namespace FinalSuspect.Modules.Resources;

public static class ResourcesDownloader
{
    public static async Task<bool> StartDownload(FileType fileType, string file)
    {
        return await DownloadInternal(fileType, file,
            (remoteType) => GetFile(fileType, remoteType, file));
    }

    public static async Task<bool> StartDownloadAsPackage(string packageName, FileType fileType, string file)
    {
        return await DownloadInternal(fileType, file,
            (remoteType) => GetPackageFile(packageName, remoteType, file));
    }

    private static async Task<bool> DownloadInternal(
        FileType fileType,
        string file,
        Func<RemoteType, string> urlGenerator)
    {
        var currentFile = file;
        var isMusic = fileType == FileType.Musics;

        var retryTimes = IsChineseLanguageUser ? 0 : 3;

        while (true)
        {
            string filePath;
            switch (fileType)
            {
                case FileType.Images:
                case FileType.Musics:
                case FileType.Languages:
                case FileType.SoundEffects:
                    filePath = GetResourceFilesPath(fileType, currentFile);
                    break;
                case FileType.Depends:
                    filePath = GetLocalPath(LocalType.BepInEx) + currentFile;
                    break;
                case FileType.ModNews:
                case FileType.Unknown:
                default:
                    return false;
            }

            var downloadFileTempPath = filePath + ".slk";
            var success = false;
            var lastError = string.Empty;

            for (var i = retryTimes; i < 4; i++)
            {
                var remoteType = (RemoteType)i;
                var url = urlGenerator(remoteType).Replace(file, currentFile);

                if (!IsValidUrl(url))
                {
                    lastError = $"Invalid URL: {url}";
                    Error(lastError, "Download Resources", false);
                    continue;
                }

                File.Create(downloadFileTempPath).Close();
                Msg($"Start Downloading from: {url}", "Download Resources");
                Msg($"Saving file to: {filePath}", "Download Resources");

                try
                {
                    using var client = new HttpClientDownloadWithProgress(url, downloadFileTempPath);
                    await client.StartDownload();
                    Thread.Sleep(100);

                    if (IsBlockedPage(downloadFileTempPath))
                    {
                        lastError = $"BLOVKED! return HTML: {url}";
                        Error(lastError, "Download Resources", false);
                        File.Delete(downloadFileTempPath);
                        continue;
                    }

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
                            lastError = $"Failed to unzip file\n{ex.Message}";
                            Error(lastError, "Download Resources", false);
                            continue;
                        }
                    }
#if Android
                    if (fileType is FileType.Depends)
                    {
                        File.Copy(filePath, BepInCorePath + currentFile);
                    }
#endif

                    Warn($"Succeed in {url}", "Download Resources");
                    success = true;
                    break;
                }
                catch (Exception ex)
                {
                    lastError = $"Failed to download\n{ex.Message}";
                    Error(lastError, "Download Resources", false);
                    File.Delete(downloadFileTempPath);
                }
            }

            if (success) return true;
            if (!isMusic) return false;

            if (AudioManager.ConvertExtensionRemote(ref currentFile))
            {
                Msg($"尝试转换文件扩展名: {file} -> {currentFile}", "Download Resources");
                continue;
            }

            Msg($"所有扩展名都已尝试，下载失败: {lastError}", "Download Resources");
            return false;
        }
    }

    private static bool IsValidUrl(string url)
    {
        const string pattern = @"^(https?|ftp)://[^\s/$.?#].[^\s]*$";
        return Regex.IsMatch(url, pattern);
    }

    private static bool IsBlockedPage(string filePath)
    {
        try
        {
            var buffer = new byte[1024];
            using var fs = File.OpenRead(filePath);
            var bytesRead = fs.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0) return false;

            var contentStart = Encoding.UTF8.GetString(buffer, 0, bytesRead).ToLower();

            var hasHtmlTags = contentStart.Contains("<!doctype html>") ||
                              contentStart.Contains("<html>") ||
                              contentStart.Contains("<head>");

            var hasBlockKeywords = contentStart.Contains("access denied") ||
                                   contentStart.Contains("firewall") ||
                                   contentStart.Contains("captive portal") ||
                                   contentStart.Contains("authentication required") ||
                                   contentStart.Contains("blocked") ||
                                   contentStart.Contains("forbidden");

            var fileSize = new FileInfo(filePath).Length;
            var isSuspiciouslySmall = fileSize < 1024;

            var isTextFile = contentStart.Contains("text/") ||
                             contentStart.Contains("html");

            return hasHtmlTags || hasBlockKeywords || (isSuspiciouslySmall && isTextFile);
        }
        catch
        {
            return false;
        }
    }
}