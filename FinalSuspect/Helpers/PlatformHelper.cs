using System;

namespace FinalSuspect.Helpers;

public class PlatformHelper
{
    public static readonly IReadOnlyDictionary<Platforms, string> PlatformColorMap = new Dictionary<Platforms, string>
    {
        { Platforms.StandaloneEpicPC, "905CDA" },
        { Platforms.StandaloneSteamPC, "4391CD" },
        { Platforms.StandaloneMac, "e3e3e3" },
        { Platforms.StandaloneWin10, "0078d4" },
        { Platforms.StandaloneItch, "E35F5F" },
        { Platforms.IPhone, "e3e3e3" },
        { Platforms.Android, "1EA21A" },
        { Platforms.Xbox, "07ff00" },
        { Platforms.Playstation, "0014b4" },
        { Platforms.Unknown, "E57373" }
    };
    
    public static string GetPlatformNameWithColor(Platforms platform)
    {
        if (platform is Platforms.Switch)
            return "<color=#00B2FF>Nintendo</color><color=#ff0000>Switch</color>";
        return $"<color=#{PlatformColorMap[platform]}>" +
               GetString(platform) +
               "</color>";
    }
    
    public static string ColorStringWithPlatform(string str, Platforms platform)
    {
        if (platform is Platforms.Switch)
        {
            var halfLength = str.Length / 2;
            var firstHalf = str.AsSpan(0, halfLength).ToString();
            var secondHalf = str.AsSpan(halfLength).ToString();
            return $"<color=#00B2FF>{firstHalf}</color><color=#ff0000>{secondHalf}</color>";
        }
        return $"<color=#{PlatformColorMap[platform]}>" +
               str +
               "</color>";
    }
}