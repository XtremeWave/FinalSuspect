using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AmongUs.Data.Player;
using Assets.InnerNet;
using FinalSuspect.Helpers;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using static FinalSuspect.Modules.Core.Plugin.ModMainMenuManager;

namespace FinalSuspect.Patches.System;

// 参考：https://github.com/Yumenopai/TownOfHost_Y
public class ModNews
{
    public string Date;
    public uint Lang;
    public int Number;
    public string ShortTitle;
    public string SubTitle;
    public string Text;
    public string Title;

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
    private static readonly List<ModNews> allModNews = [];

    public static bool AnnouncementLoadComplete;

    private static AnnouncementPopUp.AnnounceState _lastState;

    [HarmonyPatch(typeof(AnnouncementPopUp._ShowIfNew_d__42), nameof(AnnouncementPopUp._ShowIfNew_d__42.MoveNext))]
    [HarmonyPrefix]
    public static bool AnnouncementPopupPrefix()
    {
        return AnnouncementLoadComplete;
    }


    [HarmonyPatch(typeof(PlayerAnnouncementData), nameof(PlayerAnnouncementData.SetAnnouncements))]
    [HarmonyPrefix]
    public static bool SetModAnnouncements([HarmonyArgument(0)] ref Il2CppReferenceArray<Announcement> aRange)
    {
        try
        {
            var finalAllNews = new List<Announcement>();
            allModNews.ForEach(n =>
            {
                if (n.Lang == (uint)TranslationController.Instance.currentLanguage.languageID)
                    finalAllNews.Add(n.ToAnnouncement());
            });
            finalAllNews.AddRange(aRange.Where(news => !allModNews.Any(x => x.Number == news.Number)));
            finalAllNews.Sort((a1, a2) =>
            {
                if (string.IsNullOrEmpty(a1.Date) || string.IsNullOrEmpty(a2.Date))
                    return string.IsNullOrEmpty(a1.Date) ? 1 : -1;

                return DateTime.Parse(a2.Date).CompareTo(DateTime.Parse(a1.Date));
            });
            if (finalAllNews.Count == 0)
            {
                aRange = new Il2CppReferenceArray<Announcement>(0);
            }
            else
            {
                aRange = new Il2CppReferenceArray<Announcement>(finalAllNews.Count);
                for (var i = 0; i < finalAllNews.Count; i++) aRange[i] = finalAllNews[i];
            }
        }
        catch (Exception ex)
        {
            Error($"Exception in SetModAnnouncements: {ex}", "SetModAnnouncements");
        }

        return true;
    }

    public static async Task LoadModAnnouncements()
    {
        try
        {
            if (allModNews.Count >= 1) return;
            foreach (var lang in EnumHelper.GetAllValues<SupportedLangs>())
            foreach (var target in ResourcesHelper.RemoteModNewsList)
            foreach (var url in GetInfoFileUrlList())
            {
                var task = GetAnnouncements(url + $"Assets/ModNews/{lang}/{target}");
                await task;
                var result = task.Result;
                if (!result.Item1)
                    continue;
                try
                {
                    var content = GetContentFromRes(result.Item2, lang);
                    if (content != null && !string.IsNullOrEmpty(content.Date)) allModNews.Add(content);
                }
                catch
                {
                    /* ignored */
                }

                break;
            }

            allModNews.Sort((a1, a2) =>
            {
                if (string.IsNullOrEmpty(a1.Date) || string.IsNullOrEmpty(a2.Date))
                    return string.IsNullOrEmpty(a1.Date) ? 1 : -1;

                return DateTime.Parse(a2.Date).CompareTo(DateTime.Parse(a1.Date));
            });
        }
        catch
        {
            /* ignored */
        }

        _ = new MainThreadTask(() =>
        {
            AnnouncementLoadComplete = true;
            Instance.announcementPopUp.ShowIfNew();
        }, "ReShow mod announcements");
    }

    private static async Task<(bool, string)> GetAnnouncements(string url)
    {
        try
        {
            var task = RemoteHelper.GetRemoteStringAsync(url, false, false);
            await task;
            var (result, succeed) = task.Result;
            if (!succeed) return (false, "");

            await Task.Delay(100);
            Warn($"Succeed in {url}", "SetModAnnouncements");
            return (true, result);
        }
        catch
        {
            return (false, "");
        }
    }

    private static ModNews GetContentFromRes(string content, SupportedLangs lang)
    {
        ModNews mn = new();

        var byteArray = Encoding.UTF8.GetBytes(content);
        using MemoryStream stream = new(byteArray);
        using StreamReader reader = new(stream, Encoding.UTF8);
        var text = "";
        var langId = (uint)lang;
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line!.StartsWith("#Number:"))
            {
                mn.Number = int.Parse(line.Replace("#Number:", string.Empty));
            }
            else if (line.StartsWith("#Title:"))
            {
                mn.Title = line.Replace("#Title:", string.Empty);
            }
            else if (line.StartsWith("#SubTitle:"))
            {
                mn.SubTitle = line.Replace("#SubTitle:", string.Empty);
            }
            else if (line.StartsWith("#ShortTitle:"))
            {
                mn.ShortTitle = line.Replace("#ShortTitle:", string.Empty);
            }
            else if (line.StartsWith("#Date:"))
            {
                mn.Date = line.Replace("#Date:", string.Empty);
            }
            else if (line.StartsWith("#---"))
            {
            }
            else if (line.StartsWith("# "))
            {
            }
            else
            {
                const string pattern = @"\[(.*?)\]\((.*?)\)";
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
        Info($"Number:{mn.Number}", "ModNews");
        Info($"Title:{mn.Title}", "ModNews");
        Info($"SubTitle:{mn.SubTitle}", "ModNews");
        Info($"ShortTitle:{mn.ShortTitle}", "ModNews");
        Info($"Date:{mn.Date}", "ModNews");
        return mn;
    }

    [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.Update))]
    [HarmonyPostfix]
    public static void AnnouncementPopUp_Postfix(AnnouncementPopUp __instance)
    {
        if (!AnnouncementLoadComplete)
        {
            if (AnnouncementPopUp.UpdateState > AnnouncementPopUp.AnnounceState.Fetching)
                _lastState = AnnouncementPopUp.UpdateState;
            AnnouncementPopUp.UpdateState = AnnouncementPopUp.AnnounceState.Fetching;
        }
        else
        {
            AnnouncementPopUp.UpdateState = _lastState;
        }
    }

    //Reference: https://github.com/Team-YuTeam/YuEzTools
    [HarmonyPatch(typeof(AnnouncementPanel), nameof(AnnouncementPanel.SetUp))]
    [HarmonyPostfix]
    public static void SetUpPanel(AnnouncementPanel __instance, [HarmonyArgument(0)] Announcement announcement)
    {
        if (announcement.Number < 100000) return;
        var authorLogo = new GameObject("AuthorLogo") { layer = 5 };
        authorLogo.transform.SetParent(__instance.transform);
        authorLogo.transform.localPosition = new Vector3(-0.75f, 0.2f, 0.5f);
        authorLogo.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        var sr = authorLogo.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("AuthorLogo2.png", 1700f);
        sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    }
}