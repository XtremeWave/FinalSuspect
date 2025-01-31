using UnityEngine;

namespace FinalSuspect.Helpers;

public static class ColorHelper
{
    /// <summary>将颜色转换为荧光笔颜色</summary>
    /// <param name="bright">是否将颜色调整为最大亮度。如果希望较暗的颜色保持不变，请传入 false</param>
    public static Color ToMarkingColor(this Color color, bool bright = true)
    {
        Color.RGBToHSV(color, out var h, out _, out var v);
        var markingColor = Color.HSVToRGB(h, MarkerSat, bright ? MarkerVal : v).SetAlpha(MarkerAlpha);
        return markingColor;
    }
    public static Color HexToColor(string hex)
    {
        Color color = new();
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }
    public static string ColorToHex(Color color)
    {
        Color32 color32 = color;
        return $"{color32.r:X2}{color32.g:X2}{color32.b:X2}{color32.a:X2}";
    }

    private const float MarkerSat = 1f;
    private const float MarkerVal = 1f;
    private const float MarkerAlpha = 0.2f;

    public const string TeamColor = "#cdfffd"; 
    public const string ModColor = "#cecdfd";

    public static readonly Color32 TeamColor32 = new(205, 255, 253, 255);
    public static readonly Color32 ModColor32 = new(206, 205, 253, 255);
    public static readonly Color32 HalfYellow = new(255, 255, 25, 160);
    public static readonly Color32 HalfModColor32 = new(206, 205, 253, 160);
    public static readonly Color32 FaultColor = new(229, 115, 115, 255);
    public static readonly Color32 UnmatchedColor = new(191, 255, 185, 255);
    public static readonly Color32 HostNameColor = new(177, 255, 231, 255);
    public static readonly Color32 ClientlessColor = new(225, 224, 179, 255);
    public static readonly Color32 DownloadYellow = new(252, 255, 152, 255);
    public static readonly Color32 LoadCompleteGreen = new(185, 255, 181, 255);
    
    public static readonly Color32 ClientOptionColor = new(150, 149, 227, 255);
    public static readonly Color32 ClientOptionColor_Disable = new(61, 60, 97, 255);
    public static readonly Color32 ClientOptionColor_CanNotUse = new(90, 89, 108, 255);
    public static readonly Color32 ClientFeatureColor = new(191, 149, 227, 255);
    public static readonly Color32 ClientFeatureColor_ClickType = new(219, 207, 227, 255);
    public static readonly Color32 ClientFeatureColor_CanNotUse = new(102, 89, 97, 255);
    
    public static readonly Color32 ImpostorRedPale = new(255, 90, 90, 255);

    /// <summary>
    /// Darkness:１の比率で黒色と元の色を混ぜる。マイナスだと白色と混ぜる。
    /// </summary>
    public static Color ShadeColor(this Color color, float Darkness = 0)
    {
        var IsDarker = Darkness >= 0; //黒と混ぜる
        if (!IsDarker) Darkness = -Darkness;
        var Weight = IsDarker ? 0 : Darkness; //黒/白の比率
        var R = (color.r + Weight) / (Darkness + 1);
        var G = (color.g + Weight) / (Darkness + 1);
        var B = (color.b + Weight) / (Darkness + 1);
        return new Color(R, G, B, color.a);
    }
}