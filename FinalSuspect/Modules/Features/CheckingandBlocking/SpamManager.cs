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
    public static List<string> BanWords = [];

    [PluginModuleInitializer]
    public static void Init()
    {
        
        try
        {
            CreateIfNotExists();
            BanWords = ReturnAllNewLinesInFile(BANEDWORDS_FILE_PATH);
            CheckForUpdateBanWords();
            CheckForUpdateDenyNames();
        }
        catch
        {
        }
    }

    private static void CreateIfNotExists()
    {
        if (!File.Exists(BANEDWORDS_FILE_PATH))
        {
            try
            {
                if (File.Exists(@"./BanWords.txt")) File.Move(@"./BanWords.txt", BANEDWORDS_FILE_PATH);
                else
                {
                    var fileName = GetUserLangByRegion().ToString();
                    XtremeLogger.Warn($"Create New BanWords: {fileName}", "SpamManager");
                    File.WriteAllText(BANEDWORDS_FILE_PATH, GetResourcesTxt($"FinalSuspect.Resources.Configs.BanWords.{fileName}.txt"));
                }
            }
            catch (Exception ex)
            {
                XtremeLogger.Exception(ex, "SpamManager");
            }
        }

        if (File.Exists(DENY_NAME_LIST_PATH)) return;
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

    private static void CheckForUpdateBanWords()
    {
        try
        {
            var fileName = GetUserLangByRegion().ToString();
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"FinalSuspect.Resources.Configs.BanWords.{fileName}.txt");
            if (stream == null) return;
            stream.Position = 0;
            using StreamReader reader = new(stream, Encoding.UTF8);
            List<string> waitforupdate = []; 
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine().ToLower();
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
        catch 
        {
        }

    }

    private static void CheckForUpdateDenyNames()
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FinalSuspect.Resources.Configs.DenyName.txt");
        if (stream == null) return;
        stream.Position = 0;
        using StreamReader reader1 = new(stream, Encoding.UTF8);
        var existingNames = ReturnAllNewLinesInFile(DENY_NAME_LIST_PATH);
        List<string> waitforupdate = [];
        while (!reader1.EndOfStream)
        {
            var line = reader1.ReadLine();
            if (!existingNames.Contains(line))
            {
                waitforupdate.Add(line);
            }
        }
        reader1.Dispose();

        using StreamWriter writer = new(DENY_NAME_LIST_PATH, true);
        foreach (var line in waitforupdate)
        {
            writer.WriteLine(line);
        }
    }

    private static string GetResourcesTxt(string path)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        if (stream == null) return "";
        stream.Position = 0;
        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
    public static List<string> ReturnAllNewLinesInFile(string filename)
    {
        if (!File.Exists(filename)) return [];
        using StreamReader sr = new(filename, Encoding.GetEncoding("UTF-8"));
        string text;
        List<string> sendList = [];
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
        { }
    }
}
