using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FinalSuspect.Helpers;

public static class StringHelper
{
    public static readonly Encoding shiftJIS = CodePagesEncodingProvider.Instance.GetEncoding("Shift_JIS");

    /// <summary>给字符串添加荧光笔样式的装饰</summary>
    /// <param name="self">字符串</param>
    /// <param name="color">原始颜色，将自动转换为半透明的荧光色</param>
    /// <param name="bright">是否设置为最大亮度。如果想要保持较暗的颜色不变，则设置为false</param>
    /// <returns>标记后的字符串</returns>
    public static string Mark(this string self, Color color, bool bright = true)
    {
        var markingColor = color.ToMarkingColor(bright);
        var markingColorCode = ColorUtility.ToHtmlStringRGBA(markingColor);
        return $"<mark=#{markingColorCode}>{self}</mark>";
    }
    /// <summary>
    /// 计算使用SJIS编码时的字节数
    /// </summary>
    public static int GetByteCount(this string self) => shiftJIS.GetByteCount(self);

    public static string RemoveHtmlTags(this string str) => Regex.Replace(str, "<[^>]*?>", string.Empty);
    public static string RemoveHtmlTagsExcept(this string str, string exceptionLabel) => Regex.Replace(str, "<(?!/*" + exceptionLabel + ")[^>]*?>", string.Empty);
    public static string RemoveColorTags(this string str) => Regex.Replace(str, "</?color(=#[0-9a-fA-F]*)?>", "");
    public static string ColorString(Color32 color, string str) => $"<color=#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}>{str}</color>";
}