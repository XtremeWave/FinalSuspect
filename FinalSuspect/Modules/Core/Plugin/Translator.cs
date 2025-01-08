using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Resources;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using YamlDotNet.RepresentationModel;
using Object = Il2CppSystem.Object;

namespace FinalSuspect.Modules.Core.Plugin;

public static class Translator
{
    public static Dictionary<int, Dictionary<string, string>> TranslateMaps = new();
    public const string LANGUAGE_FOLDER_NAME = PathManager.LocalPath_Data + "Language";
    
    public static void Init()
    {
        XtremeLogger.Info("加载语言文件...", "Translator");
        LoadLangs();
        XtremeLogger.Info("加载语言文件成功", "Translator");
    }
    public static void LoadLangs()
    {
        var fileNames = Directory.GetFiles(PathManager.GetLocalPath(LocalType.Resources) + "Languages");
        foreach (var file in fileNames)
        {
            var fileName = Path.GetFileName(file);
            var yaml = new YamlStream();
            yaml.Load(new StringReader(new StreamReader(file).ReadToEnd()));
            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

            int langId = -1;
            var dic = new Dictionary<string, string>();

            foreach (var entry in mapping.Children)
            {
                (string key, string value) = (((YamlScalarNode)entry.Key).Value, ((YamlScalarNode)entry.Value).Value);

                if (key == "LangID")
                {
                    langId = int.Parse(value);
                    continue;
                }

                if (!dic.TryAdd(key, value))
                    XtremeLogger.Warn($"翻译文件 [{fileName}] 出现重复字符串 => {key} / {value}", "Translator");
            }

            if (langId != -1)
            {
                TranslateMaps.Remove(langId);
                TranslateMaps.Add(langId, dic);
            }
            else
                XtremeLogger.Error($"翻译文件 [{fileName}] 没有提供语言ID", "Translator");
        }

        // カスタム翻訳ファイルの読み込み
        if (!Directory.Exists(LANGUAGE_FOLDER_NAME)) Directory.CreateDirectory(LANGUAGE_FOLDER_NAME);

        // 翻訳テンプレートの作成
        CreateTemplateFile();
        foreach (var lang in EnumHelper.GetAllValues<SupportedLangs>())
        {
            if (File.Exists(@$"./{LANGUAGE_FOLDER_NAME}/{lang}.dat"))
                LoadCustomTranslation($"{lang}.dat", lang);
        }
    }

    // ReSharper restore Unity.ExpensiveCode
    public static string GetString(string s, Dictionary<string, string> replacementDic = null, bool console = false)
    {
        var langId = TranslationController.Instance?.currentLanguage?.languageID ?? GetUserLangByRegion();
        if (console) langId = SupportedLangs.SChinese;
        string str = GetString(s, langId);
        if (replacementDic != null)
            foreach (var rd in replacementDic)
                str = str.Replace(rd.Key, rd.Value);
        return str;
    }

    public static string GetString(string str, SupportedLangs langId)
    {
        var res = $"<STRMISS:{str}>";
        
        try
        {
            // 在当前语言中寻找翻译
            if (TranslateMaps[(int)langId].TryGetValue(str, out var trans))
                res = trans;
            // 繁中用户寻找简中翻译替代
            else if (langId is SupportedLangs.TChinese && TranslateMaps[(int)SupportedLangs.SChinese].TryGetValue(str, out trans))
                res = "*" + trans;
            // 非中文用户寻找英语翻译替代
            else if (langId is not SupportedLangs.English and not SupportedLangs.TChinese && TranslateMaps[(int)SupportedLangs.English].TryGetValue(str, out trans))
                res = "*" + trans;
            // 非中文用户寻找中文（原生）字符串替代
            else if (langId is not SupportedLangs.SChinese && TranslateMaps[(int)SupportedLangs.SChinese].TryGetValue(str, out trans))
                res = "*" + trans;
            // 在游戏自带的字符串中寻找
            else
            {
                var stringNames = EnumHelper.GetAllValues<StringNames>().Where(x => x.ToString() == str);
                if (stringNames != null && stringNames.Any())
                    res = GetString(stringNames.FirstOrDefault());
            }
        }
        catch (Exception Ex)
        {
            XtremeLogger.Fatal($"Error oucured at [{str}] in yaml", "Translator");
            XtremeLogger.Error("Error:\n" + Ex, "Translator");
        }
        return res;
    }
    public static string GetString(StringNames stringName)
        => DestroyableSingleton<TranslationController>.Instance.GetString(stringName, new Il2CppReferenceArray<Object>(0));
    public static string GetRoleString(string str, bool forUser = true)
    {
        var CurrentLanguage = TranslationController.Instance?.currentLanguage?.languageID ?? SupportedLangs.English;
        var lang = forUser ? CurrentLanguage : SupportedLangs.SChinese;

        return GetString(str, lang);
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
    public static bool IsChineseUser => GetUserLangByRegion() == SupportedLangs.SChinese;
    public static bool IsChineseLanguageUser => GetUserLangByRegion() is SupportedLangs.SChinese or SupportedLangs.TChinese;
    public static void LoadCustomTranslation(string filename, SupportedLangs lang)
    {
        string path = @$"./{LANGUAGE_FOLDER_NAME}/{filename}";
        if (File.Exists(path))
        {
            XtremeLogger.Info($"加载自定义翻译文件：{filename}", "LoadCustomTranslation");
            using StreamReader sr = new(path, Encoding.GetEncoding("UTF-8"));
            string text;
            string[] tmp = Array.Empty<string>();
            while ((text = sr.ReadLine()) != null)
            {
                tmp = text.Split(":");
                if (tmp.Length > 1 && tmp[1] != "")
                {
                    try
                    {
                        TranslateMaps[(int)lang][tmp[0]] = tmp.Skip(1).Join(delimiter: ":").Replace("\\n", "\n").Replace("\\r", "\r");
                    }
                    catch (KeyNotFoundException)
                    {
                        XtremeLogger.Warn($"无效密钥：{tmp[0]}", "LoadCustomTranslation");
                    }
                }
            }
        }
        else
        {
            XtremeLogger.Error($"找不到自定义翻译文件：{filename}", "LoadCustomTranslation");
        }
    }

    private static void CreateTemplateFile()
    {
        var sb = new StringBuilder();
        foreach (var title in TranslateMaps) sb.Append($"{title.Key}:\n");
        File.WriteAllText(@$"./{LANGUAGE_FOLDER_NAME}/template.dat", sb.ToString());
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
        File.WriteAllText(@$"./{LANGUAGE_FOLDER_NAME}/export_{lang}.dat", sb.ToString());
    }
}