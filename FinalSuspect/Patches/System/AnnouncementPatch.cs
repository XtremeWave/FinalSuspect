using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AmongUs.Data;
using AmongUs.Data.Player;
using Assets.InnerNet;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Modules.Resources;
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
    public static List<ModNews> AllModNews = [];

    public static ModNews GetContentFromRes(string path, SupportedLangs lang)
    {
        ModNews mn = new();

        using StreamReader reader = new(path, Encoding.UTF8);
        var text = "";
        var langId = (uint)lang;
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
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
                var pattern = @"\[(.*?)\]\((.*?)\)";
                var regex = new Regex(pattern);
                line = regex.Replace(line, match =>
                {
                    var content1 = match.Groups[1].Value;
                    var content2 = match.Groups[2].Value;
                    return $"<color=#cdfffd><nobr><link={content2}>{content1}</nobr></link></color> ";
                });

                if (line.StartsWith("## ")) line = line.Replace("## ", "<b>") + "</b>";
                else if (line.StartsWith("- ") && !line.StartsWith(" - ")) line = line.Replace("- ", "・");

                text += $"{line}\n";
            }
        }

        mn.Lang = langId;
        mn.Text = text;
        XtremeLogger.Info($"Number:{mn.Number}", "ModNews");
        XtremeLogger.Info($"Title:{mn.Title}", "ModNews");
        XtremeLogger.Info($"SubTitle:{mn.SubTitle}", "ModNews");
        XtremeLogger.Info($"ShortTitle:{mn.ShortTitle}", "ModNews");
        XtremeLogger.Info($"Date:{mn.Date}", "ModNews");
        return mn;
    }


[HarmonyPatch(typeof(PlayerAnnouncementData), nameof(PlayerAnnouncementData.SetAnnouncements)), HarmonyPrefix]
public static bool SetModAnnouncements(PlayerAnnouncementData __instance, [HarmonyArgument(0)] ref Il2CppReferenceArray<Announcement> aRange)
{
    try
    {
        // 如果 AllModNews 为空，加载所有语言的 ModNews
        if (AllModNews.Count < 1)
        {
            foreach (var lang in EnumHelper.GetAllValues<SupportedLangs>())
            {
                var fileNames = Directory.GetFiles(PathManager.GetResourceFilesPath(FileType.ModNews, lang + "/"));
                foreach (var file in fileNames)
                {
                    try
                    {
                        var content = GetContentFromRes(file, lang);
                        if (content != null && !string.IsNullOrEmpty(content.Date))
                        {
                            AllModNews.Add(content);
                        }
                    }
                    catch (Exception ex)
                    {
                        XtremeLogger.Error($"Failed to load mod news from file {file}: {ex}","");
                    }
                }
            }

            // 对 AllModNews 进行排序，处理可能的空值
            AllModNews.Sort((a1, a2) =>
            {
                if (string.IsNullOrEmpty(a1.Date) || string.IsNullOrEmpty(a2.Date))
                {
                    return string.IsNullOrEmpty(a1.Date) ? 1 : -1;
                }
                return DateTime.Parse(a2.Date).CompareTo(DateTime.Parse(a1.Date));
            });
        }
        
        List<Announcement> FinalAllNews = new List<Announcement>();
        AllModNews.ForEach(n =>
        {
            if (n.Lang == (uint)TranslationController.Instance.currentLanguage.languageID)
                FinalAllNews.Add(n.ToAnnouncement());
        });

        foreach (var news in aRange)
        {
            if (!AllModNews.Any(x => x.Number == news.Number))
            {
                FinalAllNews.Add(news);
            }
        }
        
        FinalAllNews.Sort((a1, a2) =>
        {
            if (string.IsNullOrEmpty(a1.Date) || string.IsNullOrEmpty(a2.Date))
            {
                return string.IsNullOrEmpty(a1.Date) ? 1 : -1;
            }
            return DateTime.Parse(a2.Date).CompareTo(DateTime.Parse(a1.Date));
        });

        if (FinalAllNews.Count == 0)
        {
            aRange = new Il2CppReferenceArray<Announcement>(0); 
        }
        else
        {
            aRange = new Il2CppReferenceArray<Announcement>(FinalAllNews.Count);
            for (var i = 0; i < FinalAllNews.Count; i++)
            {
                aRange[i] = FinalAllNews[i];
            }
        }

        return true;
    }
    catch (Exception ex)
    {
        XtremeLogger.Error($"Exception in SetModAnnouncements: {ex}", "");
        return true;
    }
}    static Sprite TeamLogoSprite = Utils.LoadSprite("TeamLogo.png", 1000f);
    
    //YuEzTool
    [HarmonyPatch(typeof(AnnouncementPanel), nameof(AnnouncementPanel.SetUp)), HarmonyPostfix]
    public static void SetUpPanel(AnnouncementPanel __instance, [HarmonyArgument(0)] Announcement announcement)
    {
        if (announcement.Number < 100000) return;
        var teamLogo = new GameObject("TeamLogo") { layer = 5 };
        teamLogo.transform.SetParent(__instance.transform);
        teamLogo.transform.localPosition = new Vector3(-0.81f, 0.16f, 0.5f);
        teamLogo.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        var sr = teamLogo.AddComponent<SpriteRenderer>();
        sr.sprite = TeamLogoSprite;
        sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

    }
}


