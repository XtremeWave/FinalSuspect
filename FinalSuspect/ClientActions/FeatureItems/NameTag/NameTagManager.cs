using System;
using System.IO;
using System.Text;
using AmongUs.Data;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using FinalSuspect.Modules.Resources;
using Il2CppSystem.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace FinalSuspect.ClientActions.FeatureItems.NameTag;

public static class NameTagManager
{
    public static readonly string TAGS_DIRECTORY_PATH = GetLocalPath(LocalType.NameTag);
    private static Dictionary<string, NameTag> NameTags = new();
    public static IReadOnlyDictionary<string, NameTag> AllNameTags => NameTags;

    public static IReadOnlyDictionary<string, NameTag> AllInternalNameTags =>
        AllNameTags.Where(t => t.Value.Isinternal).ToDictionary(x => x.Key, x => x.Value);

    public static IReadOnlyDictionary<string, NameTag> AllExternalNameTags =>
        AllNameTags.Where(t => !t.Value.Isinternal).ToDictionary(x => x.Key, x => x.Value);

    public static NameTag DeepClone(NameTag tag)
    {
        return new NameTag
        {
            Title = CloneCom(tag.Title),
            Prefix = CloneCom(tag.Prefix),
            Suffix = CloneCom(tag.Suffix),
            Name = CloneCom(tag.Name),
            DisplayName = CloneCom(tag.DisplayName),
            LastTag = CloneCom(tag.LastTag)
        };

        static Component CloneCom(Component com)
        {
            return com == null
                ? null
                : new Component
                {
                    Text = com.Text,
                    SizePercentage = com.SizePercentage,
                    TextColor = com.TextColor,
                    Gradient = com.Gradient != null ? new ColorGradient(com.Gradient.Colors.ToArray()) : null,
                    Spaced = com.Spaced
                };
        }
    }

    public static (string title, string prefix, string suffix, string name, string displayName, string lastTag)
        ApplyFor(PlayerControl player)
    {
        var a = AllNameTags.TryGetValue(player.FriendCode, out var tag);

        return a
            ? tag.Apply(player.GetDataName())
            : ("", "", "", "", "", "");
    }

    public static void ReloadTag(string friendCode)
    {
        if (friendCode == null)
        {
            Init();
            return;
        }

        NameTags.Remove(friendCode);
        var path = Path.Combine(TAGS_DIRECTORY_PATH, $"{friendCode}.json");
        if (File.Exists(path))
            try
            {
                ReadTagsFromFile(path);
            }
            catch (Exception ex)
            {
                Error($"Load Tag From: {path} Failed\n{ex}", "NameTagManager", false);
            }
    }

    public static void Init()
    {
        NameTags = new Dictionary<string, NameTag>();

        if (!Directory.Exists(TAGS_DIRECTORY_PATH))
            Directory.CreateDirectory(TAGS_DIRECTORY_PATH);

        foreach (var file in Directory.EnumerateFiles(TAGS_DIRECTORY_PATH, "*.json", SearchOption.AllDirectories))
        {
            if (file.Contains("template", StringComparison.OrdinalIgnoreCase)) continue;

            try
            {
                ReadTagsFromFile(file);
            }
            catch (Exception ex)
            {
                Error($"Load Tag From: {file} Failed\n{ex}", "NameTagManager", false);
            }
        }
    }

    public static void ReadTagsFromFile(string path)
    {
        var text = File.ReadAllText(path);
        var obj = JObject.Parse(text);
        var tag = GetTagFromJObject(obj);
        var friendCode = Path.GetFileNameWithoutExtension(path);

        if (tag != null && !string.IsNullOrEmpty(friendCode))
        {
            NameTags[friendCode] = tag;
            Info($"Name Tag Loaded: {friendCode}", "NameTagManager");
        }
    }

    public static NameTag GetTagFromJObject(JObject obj)
    {
        var tag = new NameTag();
        var componentMap = new Dictionary<string, Action<JToken>>
        {
            ["Title"] = token => tag.Title = GetComponent(token),
            ["Prefix"] = token => tag.Prefix = GetComponent(token),
            ["Suffix"] = token => tag.Suffix = GetComponent(token),
            ["Name"] = token => tag.Name = GetComponent(token),
            ["DisplayName"] = token => tag.DisplayName = GetComponent(token),
            ["LastTag"] = token => tag.LastTag = GetComponent(token)
        };

        foreach (var prop in obj.Properties().ToList())
            if (componentMap.TryGetValue(prop.Name, out var action))
                action(prop.Value);

        return tag;
    }

    private static Component GetComponent(JToken token)
    {
        if (token == null) return null;
        return new Component
        {
            Text = token["Text"]?.ToString(),
            SizePercentage = ParseSize(token["SizePercentage"]?.ToString()),
            TextColor = ParseColor(token["Color"]?.ToString()),
            Gradient = ParseGradient(token["Gradient"]?.ToString()),
            Spaced = token["Spaced"]?.ToString()?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false
        };
    }

    private static float? ParseSize(string str)
    {
        return float.TryParse(str, out var size) ? size : 90f;
    }

    private static Color32? ParseColor(string str)
    {
        if (string.IsNullOrEmpty(str)) return null;
        if (!str.StartsWith("#")) str = "#" + str;
        return ColorUtility.TryParseHtmlString(str, out var color) ? color : null;
    }

    private static ColorGradient ParseGradient(string str)
    {
        if (string.IsNullOrEmpty(str)) return null;

        var colors = new List<Color>();
        foreach (var colorStr in str.Split(',', '，'))
        {
            var trimmed = colorStr.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            var formatted = trimmed.StartsWith("#") ? trimmed : "#" + trimmed;
            if (ColorUtility.TryParseHtmlString(formatted, out var color))
                colors.Add(color);
        }

        return colors.Count >= 2 ? new ColorGradient(colors.ToArray()) : null;
    }

    public class NameTag
    {
        public bool Isinternal { get; set; } = false;
        public Component DisplayName { get; set; }
        public Component Title { get; set; }
        public Component Prefix { get; set; }
        public Component Suffix { get; set; }
        public Component Name { get; set; }
        public Component LastTag { get; set; }

        public (string title, string prefix, string suffix, string name, string displayName, string lastTag)
            Apply(string name, bool preview = false)
        {
            if (Name != null && Name.Text != "") name = Name.Generate(false);

            if (name == "")
                name = DataManager.player.Customization.Name;
            else if (name != "" && Name is { Text: "" })
                Name.Text = name;


            if (!preview)
                return (
                    Title?.Generate(false) ?? "",
                    Prefix?.Generate(false) ?? "",
                    Suffix?.Generate(false) ?? "",
                    name,
                    DisplayName?.Generate(false) ?? "",
                    LastTag?.Generate(false) ?? ""
                );


            name = $"{Prefix?.Generate()}{name}{Suffix?.Generate()}";
            var dp = DisplayName?.Generate(false);
            var title = dp.RemoveHtmlTags() == "" ? "" : $"({dp})";
            return ($"{name}{title}", "", "", "", "", "");
        }
    }

    public class Component
    {
        public float? SizePercentage { get; set; } = 90f;
        public string Text { get; set; }
        public Color32? TextColor { get; set; }
        public ColorGradient Gradient { get; set; }
        public bool Spaced { get; set; } = true;

        public string Generate(bool applySpace = true, bool applySize = true)
        {
            if (string.IsNullOrEmpty(Text)) return "";

            var result = Text;
            if (Gradient is { IsValid: true })
                result = Gradient.Apply(result);
            else if (TextColor != null)
                result = StringHelper.ColorString(TextColor.Value, result);

            if (Spaced && applySpace)
                result = $" {result} ";
            if (SizePercentage != null && applySize)
                result = $"<size={SizePercentage}%>{result}</size>";

            return result;
        }
    }

    public class ColorGradient
    {
        public ColorGradient(params Color[] colors)
        {
            Colors = new List<Color>(colors);
            Spacing = Colors.Count > 1 ? 1f / (Colors.Count - 1) : 0f;
        }

        public List<Color> Colors { get; }
        private float Spacing { get; }

        public bool IsValid => Colors.Count >= 2;

        public string Apply(string input)
        {
            if (input.Length == 0) return input;
            if (input.Length == 1) return StringHelper.ColorString(Colors[0], input);

            var step = 1f / (input.Length - 1);
            var sb = new StringBuilder();

            for (var i = 0; i < input.Length; i++)
            {
                var color = Evaluate(step * i);
                sb.Append(StringHelper.ColorString(color, input[i].ToString()));
            }

            return sb.ToString();
        }

        public Color Evaluate(float percent)
        {
            percent = Mathf.Clamp01(percent);
            var indexLow = Mathf.FloorToInt(percent / Spacing);

            if (indexLow >= Colors.Count - 1)
                return Colors[^1];

            var indexHigh = indexLow + 1;
            var t = (percent - indexLow * Spacing) / Spacing;

            return Color.Lerp(Colors[indexLow], Colors[indexHigh], t);
        }
    }
}