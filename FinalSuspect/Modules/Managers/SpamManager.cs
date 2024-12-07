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
        CheckForUpdate();
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
    public static void CheckForUpdate()
    {
        string fileName = GetUserLangByRegion().ToString();
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"FinalSuspect.Resources.Configs.BanWords.{fileName}.txt");
        stream.Position = 0;
        using StreamReader reader = new(stream, Encoding.UTF8);
        List<string> waitforupdate = new(); 
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            if (!BanWords.Contains(line))
            {
                waitforupdate.Add(line);
                BanWords.Add(line);
            }
        }
        reader.Dispose();

        using StreamWriter writer = new(BANEDWORDS_FILE_PATH, true);
        foreach (var line in waitforupdate)
            writer.WriteLine(line);
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
            if (text.Length >= 1 && text != "") sendList.Add(text.Replace("\\n", "\n").ToLower());
        return sendList;
    }
    public static bool CheckSpam(ref string text)
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
                }
                text = "<color=#ff1919>" + text + "</color>";
            }
            return banned;
        }
        catch 
        {
            return false;
        }
    }
}
