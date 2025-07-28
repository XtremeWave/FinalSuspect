using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AmongUs.Data;
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
    private static readonly List<ModNews> AllModNews = [];

    public static bool AnnouncementLoadComplete;

    [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.Show))]
    [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.Init))]
    [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.ShowIfNew))]
    [HarmonyPrefix]
    public static bool AnnouncementPopupPrefix()
    {
        return AnnouncementLoadComplete;
    }

    [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.Show))]
    [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.Init))]
    [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.ShowIfNew))]
    [HarmonyPostfix]
    public static void AnnouncementPopupPostfix()
    {
        if (!AnnouncementLoadComplete) Instance.announcementPopUp.Close();
    }


    [HarmonyPatch(typeof(PlayerAnnouncementData), nameof(PlayerAnnouncementData.SetAnnouncements))]
    [HarmonyPrefix]
    public static bool SetModAnnouncements([HarmonyArgument(0)] ref Il2CppReferenceArray<Announcement> aRange)
    {
        try
        {
            var FinalAllNews = new List<Announcement>();
            AllModNews.ForEach(n =>
            {
                if (n.Lang == (uint)TranslationController.Instance.currentLanguage.languageID)
                    FinalAllNews.Add(n.ToAnnouncement());
            });
            FinalAllNews.AddRange(aRange.Where(news => !AllModNews.Any(x => x.Number == news.Number)));
            FinalAllNews.Sort((a1, a2) =>
            {
                if (string.IsNullOrEmpty(a1.Date) || string.IsNullOrEmpty(a2.Date))
                    return string.IsNullOrEmpty(a1.Date) ? 1 : -1;

                return DateTime.Parse(a2.Date).CompareTo(DateTime.Parse(a1.Date));
            });

            if (FinalAllNews.Count == 0)
            {
                aRange = new Il2CppReferenceArray<Announcement>(0);
            }
            else
            {
                aRange = new Il2CppReferenceArray<Announcement>(FinalAllNews.Count);
                for (var i = 0; i < FinalAllNews.Count; i++) aRange[i] = FinalAllNews[i];
            }
        }
        catch (Exception ex)
        {
            Error($"Exception in SetModAnnouncements: {ex}", "SetModAnnouncements");
        }

        return true;
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

    public static async Task LoadModAnnouncements()
    {
        try
        {
            // 如果 AllModNews 为空，加载所有语言的 ModNews
            if (AllModNews.Count >= 1) return;
            foreach (var lang in EnumHelper.GetAllValues<SupportedLangs>())
            foreach (var target in ResourcesHelper.RemoteModNewsList)
            foreach (var url in GetInfoFileUrlList())
            {
                var task = GetAnnouncements(url + $"Assets/ModNews/{lang}/{target}", target);
                await task;
                var result = task.Result;
                if (!result.Item1)
                    continue;
                try
                {
                    var content = GetContentFromRes(result.Item2, lang);
                    if (content != null && !string.IsNullOrEmpty(content.Date)) AllModNews.Add(content);
                }
                catch
                {
                    /* ignored */
                }

                break;
            }

            // 对 AllModNews 进行排序，处理可能的空值
            AllModNews.Sort((a1, a2) =>
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
            DataManager.Player.Announcements.AllAnnouncements.Clear();
            if (!Instance) return;
            try
            {
                Instance.announcementPopUp.Show();
                Info("Loading mod announcements complete.", "SetModAnnouncements");
            }
            catch
            {
                /* ignored */
            }
        }, "ReShow mod announcements");
    }

    private static async Task<(bool, string)> GetAnnouncements(string url, string name)
    {
        try
        {
            string result;
            if (url.StartsWith("file:///"))
            {
                try
                {
                    // Windows 格式
                    var filePath = url[8..].Replace('/', '\\');
                    result = await File.ReadAllTextAsync(filePath);
                }
                catch (FileNotFoundException)
                {
                    Warn($"服务器文件缺失: {url[8..]}", "GetAnnouncements");
                    return (false, "");
                }
                catch (Exception ex)
                {
                    Error($"读取本地文件失败: {ex.Message}", "GetAnnouncements");
                    return (false, "");
                }
            }
            else
            {
                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("User-Agent", "FinalSuspect" + name);
                client.DefaultRequestHeaders.Add("Referer", "gitee.com");

                using var response = await client.GetAsync(new Uri(url), HttpCompletionOption.ResponseContentRead);
                if (!response.IsSuccessStatusCode)
                {
                    Error($"服务器请求失败 [{url}]: {response.StatusCode}", "GetAnnouncements");
                    return (false, "");
                }

                result = await response.Content.ReadAsStringAsync();
            }

            await Task.Delay(100);
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
            else if (line.StartsWith("#LangId:"))
            {
                langId = uint.Parse(line.Replace("#LangId:", string.Empty));
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
                continue;
            }
            else if (line.StartsWith("# "))
            {
                continue;
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
}