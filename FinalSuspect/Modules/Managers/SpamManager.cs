using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FinalSuspect.Attributes;
using static FinalSuspect.Translator;

namespace FinalSuspect.Modules.Managers;

public static class SpamManager
{
    private static readonly string BANEDWORDS_FILE_PATH = "./Final Suspect_Data/BanWords.txt";
    public static List<string> BanWords = new();

    [PluginModuleInitializer]
    public static void Init()
    {
        CreateIfNotExists();
        BanWords = ReturnAllNewLinesInFile(BANEDWORDS_FILE_PATH);
    }
    public static void CreateIfNotExists()
    {
        if (!File.Exists(BANEDWORDS_FILE_PATH))
        {
            try
            {
                if (!Directory.Exists(@"Final Suspect_Data")) Directory.CreateDirectory(@"Final Suspect_Data");
                if (File.Exists(@"./BanWords.txt")) File.Move(@"./BanWords.txt", BANEDWORDS_FILE_PATH);
                else
                {
                    string fileName = GetUserLangByRegion().ToString();
                    Logger.Warn($"Create New BanWords: {fileName}", "SpamManager");
                    File.WriteAllText(BANEDWORDS_FILE_PATH, GetResourcesTxt($"FinalSuspect.Resources.Configs.BanWords.{fileName}.txt"));
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "SpamManager");
            }
        }
    }
    private static string GetResourcesTxt(string path)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        stream.Position = 0;
        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
    public static List<string> ReturnAllNewLinesInFile(string filename)
    {
        if (!File.Exists(filename)) return new List<string>();
        using StreamReader sr = new(filename, Encoding.GetEncoding("UTF-8"));
        string text;
        List<string> sendList = new();
        while ((text = sr.ReadLine()) != null)
            if (text.Length > 1 && text != "") sendList.Add(text.Replace("\\n", "\n").ToLower());
        return sendList;
    }
    public static void CheckSpam(PlayerControl player, ref string text)
    {
        try
        {
            var mt = text;
            bool banned = BanWords.Any(mt.Contains);

            if (banned)
            {
                foreach (string word in BanWords)
                {
                    if (text.Contains(word))
                    {
                        text = text.Replace(word, new string('*', word.Length));
                    }
                    Logger.Test(text);

                }
                Logger.Test(text);

                text = "<color=#ff1919>" + text + "</color>";
                Logger.Test(text);

            }
        }
        catch { }
    }
}
