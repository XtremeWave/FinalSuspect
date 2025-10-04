namespace FinalSuspect.Helpers;

public static class ColorHelper
{
    private const float MarkerSat = 1f;
    private const float MarkerVal = 1f;
    private const float MarkerAlpha = 0.2f;

    public const string AuthorColorHex = "#cdfffd";
    public const string FSColorHex = "#cecdfd";

    public static readonly Color32 AuthorColor = new(205, 255, 253, 255);
    public static readonly Color32 FSColor = new(206, 205, 253, 255);
    public static readonly Color32 HalfYellow = new(255, 255, 25, 160);
    public static readonly Color32 HalfFSColor = new(206, 205, 253, 160);
    public static readonly Color32 FaultColor = new(229, 115, 115, 255);
    public static readonly Color32 UnmatchedColor = new(191, 255, 185, 255);
    public static readonly Color32 HostNameColor = new(177, 255, 231, 255);
    public static readonly Color32 ClientlessColor = new(225, 224, 179, 255);
    public static readonly Color32 DownloadYellow = new(252, 255, 152, 255);
    public static readonly Color32 CompleteGreen = new(185, 255, 181, 255);

    public static readonly Color32 FSClientOptionColor = new(150, 149, 227, 255);
    public static readonly Color32 FSClientOptionColor_Disable = new(61, 60, 97, 255);
    public static readonly Color32 FSClientOptionColor_CanNotUse = new(90, 89, 108, 255);
    public static readonly Color32 FSClientFeatureColor = new(191, 149, 227, 255);
    public static readonly Color32 FSClientFeatureColor_ClickType = new(219, 207, 227, 255);
    public static readonly Color32 FSClientFeatureColor_CanNotUse = new(102, 89, 97, 255);

    public static readonly Color32 ImpostorRedPale = new(255, 90, 90, 255);

    /// <summary>将颜色转换为荧光笔颜色</summary>
    /// <param name="color">颜色</param>
    /// <param name="bright">是否将颜色调整为最大亮度。如果希望较暗的颜色保持不变，请传入 false</param>
    public static Color ToMarkingColor(this Color color, bool bright = true)
    {
        Color.RGBToHSV(color, out var h, out _, out var v);
        var markingColor = Color.HSVToRGB(h, MarkerSat, bright ? MarkerVal : v).SetAlpha(MarkerAlpha);
        return markingColor;
    }

    public static Color HexToColor(string hex)
    {
        _ = ColorUtility.TryParseHtmlString(hex, out var color);
        return color;
    }

    public static string ColorToHex(Color color)
    {
        Color32 color32 = color;
        return $"{color32.r:X2}{color32.g:X2}{color32.b:X2}{color32.a:X2}";
    }

    /// <summary>
    ///     Darkness: 按1的比例混合黑色与原色。负值则与白色混合。
    /// </summary>
    public static Color ShadeColor(this Color color, float Darkness = 0)
    {
        var isDarker = Darkness >= 0; //与黑色混合
        if (!isDarker) Darkness = -Darkness;
        var weight = isDarker ? 0 : Darkness; //黑/白的混合比例
        var r = (color.r + weight) / (Darkness + 1);
        var g = (color.g + weight) / (Darkness + 1);
        var b = (color.b + weight) / (Darkness + 1);
        return new Color(r, g, b, color.a);
    }

    private static void ColorToHSV(Color color, out float hue /*, out float saturation, out float value*/)
    {
        var max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
        var min = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
        var delta = max - min;

        hue = 0f;
        //saturation = 0f;
        //value = max;

        if (delta != 0)
        {
            if (Mathf.Approximately(max, color.r))
            {
                hue = (color.g - color.b) / delta;
            }
            else if (Mathf.Approximately(max, color.g))
            {
                hue = 2 + (color.b - color.r) / delta;
            }
            else
            {
                hue = 4 + (color.r - color.g) / delta;
            }

            hue *= 60;
            if (hue < 0) hue += 360;
        }

        //if (max != 0)
        //{
        //saturation = delta / max;
        //}
    }

    private static Color HSVToColor(float hue, float saturation, float value)
    {
        var i = Mathf.FloorToInt(hue / 60) % 6;
        var f = hue / 60 - Mathf.Floor(hue / 60);
        var p = value * (1 - saturation);
        var q = value * (1 - f * saturation);
        var t = value * (1 - (1 - f) * saturation);

        return i switch
        {
            0 => new Color(value, t, p),
            1 => new Color(q, value, p),
            2 => new Color(p, value, t),
            3 => new Color(p, q, value),
            4 => new Color(t, p, value),
            _ => new Color(value, p, q)
        };
    }

    public static Color ConvertToLightGray(Color color)
    {
        ColorToHSV(color, out var hue /*, out _, out _*/);
        return HSVToColor(hue, 0f, 0.9f);
    }

    public static Color GetColorByPercentage(float percentage)
    {
        return new Color(
            r: Mathf.Clamp01(0.6f + percentage * 0.008f), // 0.6->1.0
            g: Mathf.Clamp01(1.0f - percentage * 0.01f), // 1.0->0.0
            b: Mathf.Clamp01(0.6f - percentage * 0.006f) // 0.6->0.0
        );
    }
}