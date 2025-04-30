using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FinalSuspect.Modules.Core.Game;
using Newtonsoft.Json.Linq;

namespace FinalSuspect.Modules.Features.CheckingandBlocking;

public static class SpamManager
{
    private static readonly string BANEDWORDS_FILE_PATH = LocalPath_Data + "BanWords.json";
    public static readonly string DENY_NAME_LIST_PATH = GetBanFilesPath("DenyName.json");
    public static List<string> BanWords = [];

    private static readonly List<string> Targets =
    [
        "DenyName.json",
        "FACList.json",
        $"BanWords/{GetUserLangByRegion()}.json"
    ];
    
    //[PluginModuleInitializer]
    public static void Init()
    {
        try
        {
            CreateIfNotExists();
            BanWords = ReturnAllNewLinesInFile(BANEDWORDS_FILE_PATH);
            foreach (var target in Targets)
            {
                foreach (var url in GetInfoFileUrlList())
                {
                    if (!GetConfigInfo(url + "Assets/Configs/" + target, target).GetAwaiter().GetResult()) continue;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Error(ex.ToString(), "SpamManager");
        }
    }

    private static void CreateIfNotExists()
    {
        if (!File.Exists(BANEDWORDS_FILE_PATH))
        {
            try
            {
                if (File.Exists(@"./BanWords.json")) 
                    File.Move(@"./BanWords.json", BANEDWORDS_FILE_PATH);
                else
                {
                    var fileName = GetUserLangByRegion().ToString();
                    Warn($"Create New BanWords: {fileName}", "SpamManager");
                }
            }
            catch (Exception ex)
            {
                Exception(ex, "SpamManager");
            }
        }

        if (File.Exists(DENY_NAME_LIST_PATH)) return;
        {
            try
            {
                if (File.Exists(@"./DenyName.json")) 
                    File.Move(@"./DenyName.json", DENY_NAME_LIST_PATH);
            }
            catch (Exception ex)
            {
                Exception(ex, "SpamManager");
            }
        }
    }

    public static List<string> ReturnAllNewLinesInFile(string filename)
    {
        if (!File.Exists(filename)) return [];
        using StreamReader sr = new(filename, Encoding.GetEncoding("UTF-8"));
        string text;
        List<string> sendList = [];
        while ((text = sr.ReadLine()) != null)
        if (text.Length >= 1 && text != "")
        sendList.Add(text.Replace("\\n", "\n").ToLower());
        return sendList;
    }

    public static void CheckSpam(ref string text)
    {
        if (!Main.SpamDenyWord.Value) return;
        try
        {
            var mt = text;
            var banned = BanWords.Any(mt.ToLower().Contains);

            if (banned)
            {
                foreach (var word in BanWords)
                {
                    if (text.ToLower().Contains(word.ToLower()))
                    {
                        text = text.Replace(word, $"<color=#E57373>{new string('*', word.Length)}</color>");
                    }
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    public static async Task<bool> GetConfigInfo(string url, string name)
    {
        try
        {
            string result;
            if (url.StartsWith("file:///"))
            {
                try
                {
                    // Windows 格式
                    string filePath = url[8..].Replace('/', '\\');
                    result = await File.ReadAllTextAsync(filePath);
                }
                catch (FileNotFoundException)
                {
                    Warn($"服务器文件缺失: {url[8..]}", "SpamManager");
                    return false;
                }
                catch (Exception ex)
                {
                    Error($"读取本地文件失败: {ex.Message}", "SpamManager");
                    return false;
                }
            }
            else
            {
                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("User-Agent", "FinalSuspect" + name);
                client.DefaultRequestHeaders.Add("Referer", "gitee.com");
                
                using var response = await client.GetAsync(new Uri(url), HttpCompletionOption.ResponseContentRead);
                if (!response.IsSuccessStatusCode)
                {
                    Error($"远程配置请求失败 [{url}]: {response.StatusCode}", "CheckRelease");
                    return false;
                }

                result = await response.Content.ReadAsStringAsync();
                result = result.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
            }

            // 增强JSON解析
            try
            {
                var data = JObject.Parse(result);
                ProcessBanWords(data);
                ProcessDenyNames(data);
                ProcessFacList(data);
            }
            catch (JsonException ex)
            {
                Error($"JSON 解析失败: {ex.Message}", "SpamManager");
                return false;
            }

            await Task.Delay(100);
            return true;
        }
        catch (Exception ex)
        {
            Error(ex.ToString(), "SpamManager");
            return false;
        }
    }

    private static void ProcessBanWords(JObject data)
    {
        var newWords = GetTokens(data["words"])
            .Except(BanWords, StringComparer.OrdinalIgnoreCase)
            .ToList();

        UpdateBanWords(newWords);
    }

    private static void ProcessDenyNames(JObject data)
    {
        var existingNames = ReturnAllNewLinesInFile(DENY_NAME_LIST_PATH);
        var newNames = GetTokens(data["denynames"])
            .Except(existingNames, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (newNames.Count > 0)
        {
            File.AppendAllLines(DENY_NAME_LIST_PATH, newNames);
        }
    }

    private static void ProcessFacList(JObject data)
    {
        var facList = GetTokens(data["Cheats"])
            .Concat(GetTokens(data["Griefer"]))
            .Where(ShouldAddToFacList)
            .ToList();

        BanManager.FACList.AddRange(facList);
    }

    private static List<string> GetTokens(JToken token)
    {
        // 处理空值或非数组类型
        if (token == null || token.Type != JTokenType.Array)
        {
            return [];
        }
        
        var jarray = token.Cast<JArray>();
        var tokens = new List<string>();
        for (var i = 0; i < jarray.Count; i++)
        {
            tokens.Add(jarray[i].ToString());
        }
 
        return [.. tokens
            .Select(item => item?.ToString())
            .Where(str => !string.IsNullOrEmpty(str))];
    }
   
    private static void UpdateBanWords(List<string> newWords)
    {
        if (newWords.Count == 0) return;

        BanWords.AddRange(newWords);
        File.AppendAllLines(BANEDWORDS_FILE_PATH, newWords);
    }

    private static bool ShouldAddToFacList(string line)
    {
        return !Main.AllPlayerControls
            .Where(p => p.IsDev())
            .Any(p => line.Contains(p.FriendCode, StringComparison.OrdinalIgnoreCase));
    }
}