using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FinalSuspect.Modules.Resources;

public class HttpClientDownloadWithProgress(string downloadUrl, string destinationFilePath) : IDisposable
{
    public delegate void ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded,
        double? progressPercentage);

    private HttpClient _httpClient;

    public void Dispose()
    {
        _httpClient?.Dispose();
        GC.SuppressFinalize(this);
    }

    public event ProgressChangedHandler ProgressChanged;

    public async Task StartDownload()
    {
        try
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
#if Windows
                                                   | SecurityProtocolType.Tls13
#endif
                ;

            var handler = CreateHttpClientHandler();

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(45)
            };

            ConfigureRequestHeaders();

            Info($"开始下载: {downloadUrl}", "Downloader");

            using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            await DownloadFileFromHttpResponseMessage(response);
        }
        catch (Exception ex)
        {
            Error($"下载启动失败: {ex.Message}", "Downloader");
            if (ex.InnerException != null)
            {
                Error($"内部异常: {ex.InnerException.Message}", "Downloader");
            }

            throw;
        }
    }

    private static HttpClientHandler CreateHttpClientHandler()
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            UseCookies = false,
            CheckCertificateRevocationList = false
        };

        // Android 特定的 SSL 配置
        ConfigureAndroidSSL(handler);

        return handler;
    }

    private static void ConfigureAndroidSSL(HttpClientHandler handler)
    {
        handler.ServerCertificateCustomValidationCallback = (_, cert, _, sslPolicyErrors) =>
        {
            Info($"SSL 验证: {sslPolicyErrors}", "Downloader");
            if (cert == null) return true;
            Info($"证书主题: {cert.Subject}", "Downloader");
            Info($"证书颁发者: {cert.Issuer}", "Downloader");
            Info($"证书有效期: {cert.GetEffectiveDateString()} - {cert.GetExpirationDateString()}", "Downloader");
            return true;
        };
    }

    private void ConfigureRequestHeaders()
    {
        // 清除可能存在的默认头
        _httpClient.DefaultRequestHeaders.Clear();

        // 设置移动设备 User-Agent
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Linux; Android 15; Mobile) " +
            "AppleWebKit/537.36 (KHTML, like Gecko) " +
            "Chrome/120.0.0.0 Mobile Safari/537.36");

        // 添加常用请求头
        _httpClient.DefaultRequestHeaders.Add("Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        _httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
        _httpClient.DefaultRequestHeaders.Add("Pragma", "no-cache");

        // 添加更多兼容性头
        _httpClient.DefaultRequestHeaders.Add("sec-ch-ua",
            "\"Not_A Brand\";v=\"8\", \"Chromium\";v=\"120\", \"Google Chrome\";v=\"120\"");
        _httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?1");
        _httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Android\"");
        _httpClient.DefaultRequestHeaders.Add("sec-fetch-site", "none");
        _httpClient.DefaultRequestHeaders.Add("sec-fetch-mode", "navigate");
        _httpClient.DefaultRequestHeaders.Add("sec-fetch-user", "?1");
        _httpClient.DefaultRequestHeaders.Add("sec-fetch-dest", "document");
        _httpClient.DefaultRequestHeaders.Add("upgrade-insecure-requests", "1");
    }

    private async Task DownloadFileFromHttpResponseMessage(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength;
        Info($"文件大小: {totalBytes} 字节", "Downloader");

        await using var contentStream = await response.Content.ReadAsStreamAsync();
        await ProcessContentStream(totalBytes, contentStream);
    }

    private async Task ProcessContentStream(long? totalDownloadSize, Stream contentStream)
    {
        var totalBytesRead = 0L;
        var readCount = 0L;
        var buffer = new byte[8192];
        var isMoreToRead = true;

        // 确保目标目录存在
        var directory = Path.GetDirectoryName(destinationFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fileStream = new FileStream(destinationFilePath,
            FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var lastProgressUpdate = DateTime.Now;

        do
        {
            var bytesRead = await contentStream.ReadAsync(buffer);
            if (bytesRead == 0)
            {
                isMoreToRead = false;
                TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                continue;
            }

            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));

            totalBytesRead += bytesRead;
            readCount += 1;

            var now = DateTime.Now;
            if (readCount % 50 != 0 && !((now - lastProgressUpdate).TotalMilliseconds > 200)) continue;
            TriggerProgressChanged(totalDownloadSize, totalBytesRead);
            lastProgressUpdate = now;
        } while (isMoreToRead);

        TriggerProgressChanged(totalDownloadSize, totalBytesRead);
        Info($"下载完成: {totalBytesRead} 字节", "Downloader");
    }

    private void TriggerProgressChanged(long? totalDownloadSize, long totalBytesRead)
    {
        ProgressChanged?.Invoke(totalDownloadSize, totalBytesRead,
            CalculateProgressPercentage(totalDownloadSize, totalBytesRead));
    }

    private static double? CalculateProgressPercentage(long? totalDownloadSize, long totalBytesRead)
    {
        if (totalDownloadSize is not > 0)
            return null;

        return Math.Round((double)totalBytesRead / totalDownloadSize.Value * 100, 2);
    }
}