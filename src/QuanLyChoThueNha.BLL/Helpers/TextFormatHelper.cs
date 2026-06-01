using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuanLyChoThueNha.BLL.Helpers
{
    public static class TextFormatHelper
    {
        private static readonly CultureInfo ViCulture = new CultureInfo("vi-VN");
        private static readonly Dictionary<string, string[]> SearchAliases = new Dictionary<string, string[]>
        {
            { "room", new[] { "phong", "can", "can ho", "ma phong" } },
            { "rooms", new[] { "phong", "can", "can ho" } },
            { "apartment", new[] { "can ho", "can", "phong" } },
            { "apartments", new[] { "can ho", "can", "phong" } },
            { "flat", new[] { "can ho", "can", "phong" } },
            { "house", new[] { "nha", "tro", "nha tro" } },
            { "building", new[] { "toa", "toa nha" } },
            { "tower", new[] { "toa", "toa nha" } },
            { "area", new[] { "khu vuc" } },
            { "district", new[] { "quan", "huyen" } },
            { "city", new[] { "thanh pho" } },
            { "address", new[] { "dia chi", "vi tri" } },
            { "location", new[] { "vi tri", "dia chi", "khu vuc" } },
            { "price", new[] { "gia", "gia thue" } },
            { "rent", new[] { "thue", "gia thue", "dang thue" } },
            { "deposit", new[] { "coc", "tien coc", "dat coc" } },
            { "floor", new[] { "tang" } },
            { "empty", new[] { "trong", "con trong" } },
            { "available", new[] { "trong", "con trong", "san sang" } },
            { "rented", new[] { "dang thue", "da thue" } },
            { "pending", new[] { "dang cho", "cho xu ly", "cho coc" } },
            { "amenity", new[] { "tien nghi" } },
            { "amenities", new[] { "tien nghi" } },
            { "bedroom", new[] { "phong ngu", "pn" } },
            { "bathroom", new[] { "phong tam", "wc" } },
            { "studio", new[] { "studio", "can ho nho" } }
        };

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
            var normalizedSource = NormalizeSearch(source);
            if (normalizedSource.Contains(kw)) return true;

            var terms = SplitSearchTerms(kw).ToList();
            if (terms.Count > 1)
                return terms.All(term => TermMatches(normalizedSource, term));

            foreach (var expanded in ExpandSearchTerms(kw))
                if (TermMatches(normalizedSource, expanded)) return true;

            return IsNearMatch(normalizedSource, kw);
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

        private static IEnumerable<string> ExpandSearchTerms(string keyword)
        {
            var result = new HashSet<string>();
            var words = SplitSearchTerms(keyword);

            foreach (var word in words)
            {
                string[] aliases;
                if (SearchAliases.TryGetValue(word, out aliases))
                {
                    foreach (var alias in aliases)
                    {
                        var normalized = NormalizeSearch(alias);
                        if (!string.IsNullOrWhiteSpace(normalized)) result.Add(normalized);
                    }
                }

                foreach (var pair in SearchAliases)
                {
                    if (pair.Value.Any(alias => NormalizeSearch(alias) == word))
                    {
                        result.Add(pair.Key);
                        foreach (var alias in pair.Value)
                        {
                            var normalized = NormalizeSearch(alias);
                            if (!string.IsNullOrWhiteSpace(normalized)) result.Add(normalized);
                        }
                    }
                }
            }

            return result;
        }

        private static IEnumerable<string> SplitSearchTerms(string value)
        {
            return value.Split(new[] { ' ', '-', '_', '/', '\\', ',', '.', ';', ':', '|', '(', ')' },
                System.StringSplitOptions.RemoveEmptyEntries);
        }

        private static bool TermMatches(string normalizedSource, string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return true;
            var sourceTokens = SplitSearchTerms(normalizedSource).ToList();
            if (term.Length <= 2)
                return sourceTokens.Any(token => token == term);

            if (normalizedSource.Contains(term)) return true;

            foreach (var expanded in ExpandSearchTerms(term))
            {
                if (expanded.Length <= 2)
                {
                    if (sourceTokens.Any(token => token == expanded)) return true;
                }
                else if (normalizedSource.Contains(expanded))
                {
                    return true;
                }
            }

            return IsNearMatch(normalizedSource, term);
        }

        private static bool IsNearMatch(string source, string keyword)
        {
            if (keyword.Length < 4) return false;
            var sourceTokens = SplitSearchTerms(source);
            var keywordTokens = SplitSearchTerms(keyword);

            foreach (var kw in keywordTokens.Where(x => x.Length >= 4))
            {
                foreach (var token in sourceTokens.Where(x => x.Length >= 4))
                {
                    var limit = kw.Length <= 6 ? 1 : 2;
                    if (LevenshteinDistance(kw, token, limit) <= limit) return true;
                }
            }

            return false;
        }

        private static int LevenshteinDistance(string a, string b, int maxDistance)
        {
            if (System.Math.Abs(a.Length - b.Length) > maxDistance) return maxDistance + 1;

            var previous = new int[b.Length + 1];
            var current = new int[b.Length + 1];
            for (var j = 0; j <= b.Length; j++) previous[j] = j;

            for (var i = 1; i <= a.Length; i++)
            {
                current[0] = i;
                var rowMin = current[0];
                for (var j = 1; j <= b.Length; j++)
                {
                    var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    current[j] = System.Math.Min(
                        System.Math.Min(current[j - 1] + 1, previous[j] + 1),
                        previous[j - 1] + cost);
                    if (current[j] < rowMin) rowMin = current[j];
                }

                if (rowMin > maxDistance) return maxDistance + 1;
                var temp = previous;
                previous = current;
                current = temp;
            }

            return previous[b.Length];
        }
    }
}
