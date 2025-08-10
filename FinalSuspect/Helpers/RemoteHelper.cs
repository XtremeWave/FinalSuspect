using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinalSuspect.Helpers;

public static class RemoteHelper
{
    public static async Task<(string, bool)> GetRemoteStringAsync(string url, bool json = true,
        bool removeLineBreaks = true)
    {
        string result;
        bool isValid;

        if (url.StartsWith("file:///"))
        {
            result = await File.ReadAllTextAsync(url[8..]);
        }
        else
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

            using HttpClient client = new(handler);

            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/127.0 Safari/537.36");

            client.DefaultRequestHeaders.Add("Accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.DefaultRequestHeaders.Add("Referer", "https://gitee.com");

            using var response = await client.GetAsync(new Uri(url), HttpCompletionOption.ResponseContentRead);
            if (!response.IsSuccessStatusCode)
            {
                Error($"Failed [{url}]: {response.StatusCode}", "Get Json Failed");
                return ("", false);
            }

            result = await response.Content.ReadAsStringAsync();

            if (removeLineBreaks)
                result = result.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
        }

        if (string.IsNullOrWhiteSpace(result)) return (result, false);
        var hasInvalidChars = HasInvalidControlCharacters(result);

        if (json)
        {
            isValid = !hasInvalidChars && IsValidJson(result);
        }
        else
        {
            isValid = !hasInvalidChars;
        }

        return (result, isValid);
    }

    private static bool HasInvalidControlCharacters(string input)
    {
        var allowedWhitespace = new[] { ' ', '\t', '\n', '\r' };

        return input.Any(c => char.IsControl(c) && !allowedWhitespace.Contains(c));
    }

    private static bool IsValidJson(string str)
    {
        try
        {
            using (JsonDocument.Parse(str))
            {
                return true;
            }
        }
        catch (JsonException)
        {
            return false;
        }
    }
}