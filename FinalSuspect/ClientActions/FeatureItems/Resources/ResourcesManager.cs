using System;
using System.Text.Json;
using System.Threading.Tasks;
using FinalSuspect.Helpers;

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
            var task = JsonHelper.GetJsonStringAsync(url);
            await task;
            var (result, succeed) = task.Result;
            if (!succeed) return false;

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