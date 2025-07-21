using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinalSuspect.ClientActions.FeatureItems.Resources;

public static class ResourcesManager
{
    public static readonly Dictionary<string, List<string>> AllResources = new();

    public static async Task CheckForResources()
    {
        foreach (var url in GetInfoFileUrlList(true))
        {
            var task = GetAllResources(url + "fs_resources.json");
            await task;
            if (!task.Result) continue;
            break;
        }
    }

    private static async Task<bool> GetAllResources(string url)
    {
        try
        {
            string result;

            if (url.StartsWith("file:///"))
            {
                result = await File.ReadAllTextAsync(url[8..]);
            }
            else
            {
                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("User-Agent", "FracturedTruth Updater");
                client.DefaultRequestHeaders.Add("Referer", "www.Final.net.cn");

                using var response = await client.GetAsync(new Uri(url), HttpCompletionOption.ResponseContentRead);
                if (!response.IsSuccessStatusCode)
                {
                    Error($"Failed: {response.StatusCode}", "Check Resources");
                    return false;
                }

                result = await response.Content.ReadAsStringAsync();
                result = result.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
            }

            var data = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(result);
            foreach (var kvp in data) AllResources.Add(kvp.Key, kvp.Value);

            return true;
        }
        catch (Exception ex)
        {
            Error($"Exception: {ex.Message}", "Check Resources");
            return false;
        }
    }
}