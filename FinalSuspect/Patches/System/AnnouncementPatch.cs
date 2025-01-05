using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AmongUs.Data;
using AmongUs.Data.Player;
using Assets.InnerNet;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Modules.Resources;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace FinalSuspect.Patches.System;

// 参考：https://github.com/Yumenopai/TownOfHost_Y
public class ModNews
{
    public int Number;
    public uint Lang;
    public string Title;
    public string SubTitle;
    public string ShortTitle;
    public string Text;
    public string Date;

    public Announcement ToAnnouncement()
    {
        var result = new Announcement
        {
            Number = Number,
            Language = Lang,
            Title = Title,
            SubTitle = SubTitle,
            ShortTitle = ShortTitle,
            Text = Text,
            Date = Date,
            Id = "ModNews"
        };
        return result;
    }
}

[HarmonyPatch]
public class ModNewsHistory
{
    public static List<ModNews> AllModNews = new();
    public static ModNews GetContentFromRes(string path)
    {
        ModNews mn = new();
        
        using StreamReader reader = new(path, Encoding.UTF8);
        string text = "";
        uint langId = (uint)DataManager.Settings.Language.CurrentLanguage;
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            if (line.StartsWith("#Number:")) mn.Number = int.Parse(line.Replace("#Number:", string.Empty));
            else if (line.StartsWith("#LangId:")) langId = uint.Parse(line.Replace("#LangId:", string.Empty));
            else if (line.StartsWith("#Title:")) mn.Title = line.Replace("#Title:", string.Empty);
            else if (line.StartsWith("#SubTitle:")) mn.SubTitle = line.Replace("#SubTitle:", string.Empty);
            else if (line.StartsWith("#ShortTitle:")) mn.ShortTitle = line.Replace("#ShortTitle:", string.Empty);
            else if (line.StartsWith("#Date:")) mn.Date = line.Replace("#Date:", string.Empty);
            else if (line.StartsWith("#---")) continue;
            else if (line.StartsWith("# ")) continue;
            else
            {
                if (line.StartsWith("## ")) line = line.Replace("## ", "<b>") + "</b>";
                else if (line.StartsWith("- ") && !line.StartsWith(" - ")) line = line.Replace("- ", "・");
                text += $"\n{line}";
            }
        }
        mn.Lang = langId;
        mn.Text = text;
        Modules.Core.Plugin.Logger.Info($"Number:{mn.Number}", "ModNews");
        Modules.Core.Plugin.Logger.Info($"Title:{mn.Title}", "ModNews");
        Modules.Core.Plugin.Logger.Info($"SubTitle:{mn.SubTitle}", "ModNews");
        Modules.Core.Plugin.Logger.Info($"ShortTitle:{mn.ShortTitle}", "ModNews");
        Modules.Core.Plugin.Logger.Info($"Date:{mn.Date}", "ModNews");
        return mn;
    }

    [HarmonyPatch(typeof(PlayerAnnouncementData), nameof(PlayerAnnouncementData.SetAnnouncements)), HarmonyPrefix]
    public static bool SetModAnnouncements(PlayerAnnouncementData __instance, [HarmonyArgument(0)] ref Il2CppReferenceArray<Announcement> aRange)
    {
        if (AllModNews.Count < 1)
        {
            var lang = DataManager.Settings.Language.CurrentLanguage.ToString(); ;

            var fileNames = Directory.GetFiles(PathManager.GetResourceFilesPath(FileType.ModNews, lang + "/"));
            foreach (var file in fileNames)
                AllModNews.Add(GetContentFromRes(file));

            AllModNews.Sort((a1, a2) => { return DateTime.Compare(DateTime.Parse(a2.Date), DateTime.Parse(a1.Date)); });
        }

        List<Announcement> FinalAllNews = new();
        AllModNews.Do(n => FinalAllNews.Add(n.ToAnnouncement()));
        foreach (var news in aRange)
        {
            if (!AllModNews.Any(x => x.Number == news.Number))
                FinalAllNews.Add(news);
        }
        FinalAllNews.Sort((a1, a2) => { return DateTime.Compare(DateTime.Parse(a2.Date), DateTime.Parse(a1.Date)); });

        aRange = new(FinalAllNews.Count);
        for (int i = 0; i < FinalAllNews.Count; i++)
            aRange[i] = FinalAllNews[i];

        return true;
    }
    static Sprite TeamLogoSprite = Utils.LoadSprite($"TeamLogo.png", 1000f);
    
    //YuEzTool
    [HarmonyPatch(typeof(AnnouncementPanel), nameof(AnnouncementPanel.SetUp)), HarmonyPostfix]
    public static void SetUpPanel(AnnouncementPanel __instance, [HarmonyArgument(0)] Announcement announcement)
    {
        if (announcement.Number < 100000) return;
        var TeamLogo = new GameObject("TeamLogo") { layer = 5 };
        TeamLogo.transform.SetParent(__instance.transform);
        TeamLogo.transform.localPosition = new Vector3(-0.81f, 0.16f, 0.5f);
        TeamLogo.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        var sr = TeamLogo.AddComponent<SpriteRenderer>();
        sr.sprite = TeamLogoSprite;
        sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

    }
}


