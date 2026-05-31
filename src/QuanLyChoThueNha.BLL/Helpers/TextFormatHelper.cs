using System.Globalization;
using System.Linq;
using System.Text;

namespace QuanLyChoThueNha.BLL.Helpers
{
    public static class TextFormatHelper
    {
        private static readonly CultureInfo ViCulture = new CultureInfo("vi-VN");

        public static string NormalizeSearch(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            var normalized = value.Trim().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();
            foreach (var c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category == UnicodeCategory.NonSpacingMark) continue;
                if (c == 'đ') builder.Append('d');
                else if (c == 'Đ') builder.Append('D');
                else builder.Append(c);
            }

            return builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }

        public static bool ContainsNormalized(string source, string keyword)
        {
            var kw = NormalizeSearch(keyword);
            if (kw.Length == 0) return true;
            return NormalizeSearch(source).Contains(kw);
        }

        public static string Money(decimal value)
        {
            return value.ToString("N0", ViCulture);
        }

        public static string Money(double value)
        {
            return value.ToString("N0", ViCulture);
        }

        public static string JoinSearchParts(params object[] values)
        {
            return string.Join(" ", values
                .Where(v => v != null)
                .Select(v => v.ToString()));
        }
    }
}
