using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FinalSuspect.Modules.Resources;

// 来源：Town Of Next
public class HttpClientDownloadWithProgress(string downloadUrl, string destinationFilePath) : IDisposable
{
    public delegate void ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded,
        double? progressPercentage);

    private HttpClient _httpClient;

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    public event ProgressChangedHandler ProgressChanged;

    public async Task StartDownload()
    {
        ServicePointManager.SecurityProtocol =
            SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            UseDefaultCredentials = true,
            Proxy = null,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        // 浏览器级请求头, 完全防止403
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
            "AppleWebKit/537.36 (KHTML, like Gecko) " +
            "Chrome/127.0.0.0 Safari/537.36");

        _httpClient.DefaultRequestHeaders.Add("Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        _httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
        _httpClient.DefaultRequestHeaders.Add("Pragma", "no-cache");

        using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
        await DownloadFileFromHttpResponseMessage(response);
    }

    private async Task DownloadFileFromHttpResponseMessage(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength;

        await using var contentStream = await response.Content.ReadAsStreamAsync();
        await ProcessContentStream(totalBytes, contentStream);
    }

    private async Task ProcessContentStream(long? totalDownloadSize, Stream contentStream)
    {
        var totalBytesRead = 0L;
        var readCount = 0L;
        var buffer = new byte[8192];
        var isMoreToRead = true;

        await using var fileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write,
            FileShare.None,
            8192, true);
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

            if (readCount % 100 == 0)
                TriggerProgressChanged(totalDownloadSize, totalBytesRead);
        } while (isMoreToRead);
    }

    private void TriggerProgressChanged(long? totalDownloadSize, long totalBytesRead)
    {
        if (ProgressChanged == null)
            return;

        double? progressPercentage = null;
        if (totalDownloadSize.HasValue)
            progressPercentage = Math.Round((double)totalBytesRead / totalDownloadSize.Value * 100, 2);

        ProgressChanged(totalDownloadSize, totalBytesRead, progressPercentage);
    }
}