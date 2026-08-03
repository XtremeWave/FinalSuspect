using System;
using System.Globalization;
using System.IO;
using System.Text;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Resources;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using YamlDotNet.RepresentationModel;
using Object = Il2CppSystem.Object;

namespace FinalSuspect.Modules.Core.Plugin;

public static class Translator
{
    private static readonly Dictionary<int, Dictionary<string, string>> TranslateMaps = new();

    public static void TranslatorInit()
    {
        Info("加载语言文件...", "Translator");
        LoadLangs();
        Info("加载语言文件完成", "Translator");
    }

    public static void LoadLangs()
    {
        var langDir = Path.Combine(GetLocalPath(LocalType.Resources), "Languages");
        if (!Directory.Exists(langDir)) return;

        foreach (var filePath in Directory.GetFiles(langDir))
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var langId = -1;

            foreach (var lang in EnumHelper.GetAllValues<SupportedLangs>())
                if (fileName == lang.ToString())
                    langId = (int)lang;

            if (langId == -1)
                continue;
            try
            {
                using var reader = new StreamReader(filePath);
                var yaml = new YamlStream();
                yaml.Load(new StringReader(reader.ReadToEnd()));

                var dic = new Dictionary<string, string>();
                var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

                foreach (var entry in mapping.Children)
                {
                    var keyNode = (YamlScalarNode)entry.Key;
                    var valueNode = (YamlScalarNode)entry.Value;

                    if (keyNode.Value == "LangID") continue;

                    if (!dic.TryAdd(keyNode.Value, valueNode.Value))
                        Warn($"翻译文件 [{fileName}] 出现重复字符串: {keyNode.Value}", "Translator");
                }

                // 更新翻译映射
                TranslateMaps[langId] = dic;
                Info($"成功加载语言: {fileName} ({dic.Count} 个条目)", "Translator");
            }
            catch (Exception ex)
            {
                Error($"加载语言文件失败: {Path.GetFileName(filePath)}\n{ex.Message}", "Translator");
            }
        }

        // 处理自定义翻译
        CreateTemplateFile();
        var customLangDir = Path.Combine(".", LANGUAGE_FOLDER_NAME);

        if (!Directory.Exists(customLangDir)) return;
        {
            foreach (var lang in Enum.GetValues(typeof(SupportedLangs)).Cast<SupportedLangs>())
            {
                var customFile = Path.Combine(customLangDir, $"{lang}.dat");
                if (File.Exists(customFile)) LoadCustomTranslation(customFile, lang);
            }
        }
    }
    // ReSharper restore Unity.ExpensiveCode

    public static bool IsChineseUser => GetUserLangByRegion() == SupportedLangs.SChinese;

    public static bool IsChineseLanguageUser =>
        GetUserLangByRegion() is SupportedLangs.SChinese or SupportedLangs.TChinese;

    public static string GetString(string s, Dictionary<string, string> replacementDic = null, bool console = false)
    {
        var langId = TranslationController.Instance?.currentLanguage?.languageID ?? GetUserLangByRegion();
        if (console) langId = SupportedLangs.SChinese;
        var str = GetString(s, langId);
        if (replacementDic == null) return str;
        return replacementDic.Aggregate(str, (current, rd) => current.Replace(rd.Key, rd.Value));
    }
    
    public static string GetString<T>(T s, Dictionary<string, string> replacementDic = null, bool console = false) where T : Enum
    {
        var str = GetString($"{typeof(T).Name}.{s.ToString()}", replacementDic, console);
        return str;
    }

    private static string GetString(string str, SupportedLangs langId)
    {
        var res = $"<STRMISS:{str}>";

        try
        {
            // 在当前语言中寻找翻译
            if (TranslateMaps[(int)langId].TryGetValue(str, out var trans))
            {
                res = trans;
            }
            // 繁中用户寻找简中翻译替代
            else if (langId is SupportedLangs.TChinese &&
                     TranslateMaps[(int)SupportedLangs.SChinese].TryGetValue(str, out trans))
            {
                res = "*" + trans;
            }
            // 非中文用户寻找英语翻译替代
            else if (langId is not SupportedLangs.English and not SupportedLangs.TChinese &&
                     TranslateMaps[(int)SupportedLangs.English].TryGetValue(str, out trans))
            {
                res = "*" + trans;
            }
            // 非中文用户寻找中文（原生）字符串替代
            else if (langId is not SupportedLangs.SChinese &&
                     TranslateMaps[(int)SupportedLangs.SChinese].TryGetValue(str, out trans))
            {
                res = "*" + trans;
            }
            // 在游戏自带的字符串中寻找
            else
            {
                var stringNames = EnumHelper.GetAllValues<StringNames>().Where(x => x.ToString() == str);
                var stringNamesEnumerable = stringNames.ToList();
                if (stringNamesEnumerable.Any())
                    res = GetString(stringNamesEnumerable.FirstOrDefault());
            }
        }
        catch (Exception Ex)
        {
            Fatal($"Error oucured at [{str}] in yaml", "Translator");
            Error("Error:\n" + Ex, "Translator");
        }

        return res;
    }

    public static string GetString(StringNames stringName)
    {
        return DestroyableSingleton<TranslationController>.Instance.GetString(stringName,
            new Il2CppReferenceArray<Object>(0));
    }

    public static string GetRoleString(string str)
    {
        return GetString($"Role.{str}");
    }

    public static SupportedLangs GetUserLangByRegion()
    {
        try
        {
            var name = CultureInfo.CurrentUICulture.Name;
            if (name.StartsWith("en")) return SupportedLangs.English;
            if (name.StartsWith("ru")) return SupportedLangs.Russian;
            if (name.StartsWith("pt")) return SupportedLangs.Brazilian;
            if (name.StartsWith("zh_CHT")) return SupportedLangs.TChinese;
            if (name.StartsWith("zh")) return SupportedLangs.SChinese;
            if (name.StartsWith("ja")) return SupportedLangs.Japanese;

            return TranslationController.Instance?.currentLanguage?.languageID ?? SupportedLangs.English;
        }
        catch
        {
            return SupportedLangs.English;
        }
    }

    private static void LoadCustomTranslation(string filename, SupportedLangs lang)
    {
        var path = $"./{LANGUAGE_FOLDER_NAME}/{filename}";
        if (File.Exists(path))
        {
            Info($"加载自定义翻译文件：{filename}", "LoadCustomTranslation");
            using StreamReader sr = new(path, Encoding.GetEncoding("UTF-8"));
            while (sr.ReadLine() is { } text)
            {
                var tmp = text.Split(":");
                if (tmp.Length <= 1 || tmp[1] == "") continue;
                try
                {
                    TranslateMaps[(int)lang][tmp[0]] =
                        tmp.Skip(1).Join(delimiter: ":").Replace("\\n", "\n").Replace("\\r", "\r");
                }
                catch (KeyNotFoundException)
                {
                    Warn($"无效密钥：{tmp[0]}", "LoadCustomTranslation");
                }
            }
        }
        else
        {
            Error($"找不到自定义翻译文件：{filename}", "LoadCustomTranslation");
        }
    }

    private static void CreateTemplateFile()
    {
        var sb = new StringBuilder();
        foreach (var title in TranslateMaps) sb.Append($"{title.Key}:\n");
        File.WriteAllText($"./{LANGUAGE_FOLDER_NAME}/template.dat", sb.ToString());
    }

    public static void ExportCustomTranslation()
    {
        LoadLangs();
        var sb = new StringBuilder();
        var lang = TranslationController.Instance.currentLanguage.languageID;
        foreach (var kvp in TranslateMaps[13])
        {
            var text = kvp.Value;
            if (!TranslateMaps.ContainsKey((int)lang)) text = "";
            sb.Append($"{kvp.Key}:{text}\n");
        }

        File.WriteAllText($"./{LANGUAGE_FOLDER_NAME}/export_{lang}.dat", sb.ToString());
    }
}