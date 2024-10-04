using UnityEngine;

namespace FinalSuspect_Xtreme;

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
        Color32 color32 = (Color32)color;
        return $"{color32.r:X2}{color32.g:X2}{color32.b:X2}{color32.a:X2}";
    }

    private const float MarkerSat = 1f;
    private const float MarkerVal = 1f;
    private const float MarkerAlpha = 0.2f;

    public static readonly string TeamColor = "#cdfffd"; 
    public static readonly string ModColor = "#cecdfd";

    public static readonly Color32 TeamColor32 = new(205, 255, 253, 255);
    public static readonly Color32 ModColor32 = new(206, 205, 253, 255);
    public static readonly Color32 OutColor = new(180, 179, 231, 255);
    public static readonly Color32 HalfYellow = new(255, 255, 25, 160);
    public static readonly Color32 HalfModColor32 = new(206, 205, 253, 160);
}