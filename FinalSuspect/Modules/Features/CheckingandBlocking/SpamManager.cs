using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FinalSuspect.Attributes;
using FinalSuspect.Modules.Resources;

namespace FinalSuspect.Modules.Features.CheckingandBlocking;

public static class SpamManager
{
    private static readonly string BANEDWORDS_FILE_PATH = PathManager.LocalPath_Data + "BanWords.txt";
    public static readonly string DENY_NAME_LIST_PATH = PathManager.GetBanFilesPath("DenyName.txt");
    public static List<string> BanWords = new();

    [PluginModuleInitializer]
    public static void Init()
    {
        CreateIfNotExists();
        BanWords = ReturnAllNewLinesInFile(BANEDWORDS_FILE_PATH);
        CheckForUpdateBanWords();
        CheckForUpdateDenyNames();
    }
    public static void CreateIfNotExists()
    {
        if (!File.Exists(BANEDWORDS_FILE_PATH))
        {
            try
            {
                if (File.Exists(@"./BanWords.txt")) File.Move(@"./BanWords.txt", BANEDWORDS_FILE_PATH);
                else
                {
                    string fileName = GetUserLangByRegion().ToString();
                    XtremeLogger.Warn($"Create New BanWords: {fileName}", "SpamManager");
                    File.WriteAllText(BANEDWORDS_FILE_PATH, GetResourcesTxt($"FinalSuspect.Resources.Configs.BanWords.{fileName}.txt"));
                }
            }
            catch (Exception ex)
            {
                XtremeLogger.Exception(ex, "SpamManager");
            }
        }
        if (!File.Exists(DENY_NAME_LIST_PATH))
        {
            try
            {
                if (!Directory.Exists(@"Final Suspect_Data")) Directory.CreateDirectory(@"Final Suspect_Data");
                if (File.Exists(@"./DenyName.txt")) File.Move(@"./DenyName.txt", BANEDWORDS_FILE_PATH);
                else
                {
                    File.WriteAllText(DENY_NAME_LIST_PATH, GetResourcesTxt("FinalSuspect.Resources.Configs.DenyName.txt"));
                }
            }
            catch (Exception ex)
            {
                XtremeLogger.Exception(ex, "SpamManager");
            }
        }

    }
    public static void CheckForUpdateBanWords()
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
    public static void CheckForUpdateDenyNames()
    {
        string fileName = GetUserLangByRegion().ToString();
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FinalSuspect.Resources.Configs.DenyName.txt");
        stream.Position = 0;
        using StreamReader reader1 = new(stream, Encoding.UTF8);
        using StreamReader reader2 = new(DENY_NAME_LIST_PATH,  Encoding.UTF8);
        List<string> waitforupdate = new(); 
        while (!reader1.EndOfStream)
        {
            string line = reader1.ReadLine();
            if (!reader2.ReadToEnd().Contains(line))
                waitforupdate.Add(line);
        }
        reader1.Dispose();
        reader2.Dispose();

        using StreamWriter writer = new(DENY_NAME_LIST_PATH, true);
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
    public static void CheckSpam(ref string text)
    {
        if (!Main.SpamDenyWord.Value) return;
        try
        {
            var mt = text;
            bool banned = BanWords.Any(mt.ToLower().Contains);

            if (banned)
            {
                foreach (string word in BanWords)
                {
                    if (text.ToLower().Contains(word.ToLower()))
                    {
                        text = text.Replace(word, $"<color=#E57373>{new string('*', word.Length)}</color>");
                    }
                }
            }
        }
        catch 
        { }
    }
}
