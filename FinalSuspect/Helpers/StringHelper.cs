using System.Text;
using System.Text.RegularExpressions;

namespace FinalSuspect.Helpers;

public static class StringHelper
{
    private static readonly Encoding shiftJIS = CodePagesEncodingProvider.Instance.GetEncoding("Shift_JIS");

    public static string ColorString(Color32 color, string str)
    {
        return $"<color=#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}>{str}</color>";
    }

    /// <param name="self">字符串</param>
    extension(string self)
    {
        /// <summary>给字符串添加荧光笔样式的装饰</summary>
        /// <param name="color">原始颜色，将自动转换为半透明的荧光色</param>
        /// <param name="bright">是否设置为最大亮度。如果想要保持较暗的颜色不变，则设置为false</param>
        /// <returns>标记后的字符串</returns>
        public string Mark(Color color, bool bright = true)
        {
            var markingColor = color.ToMarkingColor(bright);
            var markingColorCode = ColorUtility.ToHtmlStringRGBA(markingColor);
            return $"<mark=#{markingColorCode}>{self}</mark>";
        }

        /// <summary>
        ///     计算使用SJIS编码时的字节数
        /// </summary>
        public int GetByteCount()
        {
            return shiftJIS.GetByteCount(self);
        }

        public string RemoveHtmlTags()
        {
            return Regex.Replace(self, "<[^>]*?>", string.Empty);
        }

        public string RemoveHtmlTagsExcept(string exceptionLabel)
        {
            return Regex.Replace(self, "<(?!/*" + exceptionLabel + ")[^>]*?>", string.Empty);
        }

        public string RemoveColorTags()
        {
            return Regex.Replace(self, "</?color(=#[0-9a-fA-F]*)?>", "");
        }
    }
}