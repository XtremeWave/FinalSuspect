using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StringWriter = Il2CppSystem.IO.StringWriter;

namespace FinalSuspect.Modules.Features.CheckingandBlocking;

public static class SpamManager
{
    private static List<string> BanWords = [];

    private static readonly List<string> Targets =
    [
        "DenyName.json",
        "FACList.json",
        $"BanWords/{GetUserLangByRegion()}.json"
    ];

    public static async Task Init()
    {
        try
        {
            BanWords = ReturnAllNewLinesInFile(BANEDWORDS_FILE_PATH);
            foreach (var target in Targets)
            foreach (var url in GetInfoFileUrlList())
            {
                var task = GetConfigs(url + "Assets/Configs/" + target);
                await task;
                if (!task.Result) continue;
                break;
            }
        }
        catch (Exception ex)
        {
            Error(ex.ToString(), "SpamManager");
        }
    }


    public static List<string> ReturnAllNewLinesInFile(string filepath)
    {
        if (!File.Exists(filepath)) return [];
        var json = File.ReadAllText(filepath);
        List<string> sendList;
        try
        {
            sendList = JsonConvert.DeserializeObject<List<string>>(json) ?? [];
        }
        catch
        {
            sendList = [];
        }

        return sendList;
    }

    private static async Task<bool> GetConfigs(string url)
    {
        try
        {
            var task = RemoteHelper.GetRemoteStringAsync(url);
            await task;
            var (result, succeed) = task.Result;
            if (!succeed) return false;

            var data = JObject.Parse(result);
            try
            {
                ProcessBanWords(data);
                ProcessDenyNames(data);
                ProcessFacList(data);
            }
            catch (Exception ex)
            {
                Error($"JSON 解析失败: {ex.Message}\nData: {result[..Math.Min(100, result.Length)]}", "SpamManager");
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
            .Select(DecryptBase64)
            .Select(DecodeUnicodeEscapes)
            .Except(BanWords, StringComparer.OrdinalIgnoreCase)
            .ToList();

        BanWords.AddRange(newWords);
        Update(newWords, BANEDWORDS_FILE_PATH);
    }

    private static void ProcessDenyNames(JObject data)
    {
        var existingNames = ReturnAllNewLinesInFile(DENY_NAME_LIST_PATH);
        var newNames = GetTokens(data["denynames"])
            .Select(DecryptBase64)
            .Select(DecodeUnicodeEscapes)
            .Except(existingNames, StringComparer.OrdinalIgnoreCase)
            .ToList();

        Update(newNames, DENY_NAME_LIST_PATH);
    }

    private static void ProcessFacList(JObject data)
    {
        var facList = GetTokens(data["Cheats"])
            .Concat(GetTokens(data["Griefer"]))
            .Select(DecryptBase64)
            .Select(DecodeUnicodeEscapes)
            .Where(ShouldAddToFacList)
            .ToList();

        BanManager.FACList.AddRange(facList);
    }

    private static List<string> GetTokens(JToken token)
    {
        if (token is not { Type: JTokenType.Array }) return [];

        var jarray = token.Cast<JArray>();
        var tokens = new List<string>();
        for (var i = 0; i < jarray.Count; i++) tokens.Add(jarray[i].ToString());

        return
        [
            .. tokens
                .Select(item => item?.ToString())
                .Where(str => !string.IsNullOrEmpty(str))
        ];
    }

    private static string DecodeUnicodeEscapes(string input)
    {
        return Regex.Replace(input, @"\\u([0-9A-Fa-f]{4})", match =>
        {
            try
            {
                return ((char)Convert.ToInt32(match.Groups[1].Value, 16)).ToString();
            }
            catch
            {
                return match.Value;
            }
        });
    }

    private static string DecryptBase64(string cipherText)
    {
        try
        {
            var bytes = Convert.FromBase64String(cipherText);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return cipherText;
        }
    }

    private static void Update(List<string> newWords, string path)
    {
        if (newWords.Count == 0) return;

        List<string> updateWords;
        if (!File.Exists(path)) return;

        try
        {
            var json = File.ReadAllText(path);
            updateWords = JsonConvert.DeserializeObject<List<string>>(json) ?? [];
        }
        catch
        {
            updateWords = [];
        }

        var allWords = updateWords.Union(newWords).ToList();

        _ = new MainThreadTask(() =>
        {
            StringWriter sw = new();
            JsonWriter jsonWriter = new JsonTextWriter(sw);

            jsonWriter.WriteStartArray();

            foreach (var word in allWords) jsonWriter.WriteValue(word);

            jsonWriter.WriteEndArray();
            sw.Flush();

            File.WriteAllText(path, sw.ToString());
        }, "Write in Ban");
    }

    private static bool ShouldAddToFacList(string line)
    {
        return !Main.AllPlayerControls
            .Where(p => p.IsDev())
            .Any(p => line.Contains(p.FriendCode, StringComparison.OrdinalIgnoreCase));
    }

    public static void CheckSpam(ref string text)
    {
        if (!ConfigManager.SpamDenyWord.Value || BanWords.Count == 0) return;

        try
        {
            var lowerText = text.ToLowerInvariant();
            var bannedWords = BanWords.Where(word => lowerText.Contains(word.ToLowerInvariant())).ToList();

            if (bannedWords.Count == 0) return;

            var pattern = string.Join("|", bannedWords.Select(Regex.Escape));
            text = Regex.Replace(text, pattern, match =>
                    $"<color=#E57373>{new string('*', match.Value.Length)}</color>",
                RegexOptions.IgnoreCase);
        }
        catch
        {
            /* ignored */
        }
    }
}