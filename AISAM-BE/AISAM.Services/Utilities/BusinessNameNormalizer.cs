using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AISAM.Services.Utilities;

public static partial class BusinessNameNormalizer
{
    private static readonly IReadOnlyList<(Regex Pattern, string Replacement)> AbbreviationRules =
    [
        (WordRegex("cty"), "cong ty"),
        (WordRegex("tnhh"), "trach nhiem huu han"),
        (WordRegex("mtv"), "mot thanh vien"),
        (WordRegex("cp"), "co phan"),
        (WordRegex("jsc"), "co phan"),
        (WordRegex("ltd"), "trach nhiem huu han"),
        (WordRegex("llc"), "trach nhiem huu han"),
        (WordRegex("tm"), "thuong mai"),
        (WordRegex("dv"), "dich vu"),
        (WordRegex("sx"), "san xuat"),
        (WordRegex("xnk"), "xuat nhap khau"),
        (WordRegex("tmdv"), "thuong mai dich vu"),
        (WordRegex("sxtm"), "san xuat thuong mai")
    ];

    public static string NormalizeBusinessName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        var normalized = RemoveVietnameseDiacritics(name)
            .ToLowerInvariant()
            .Replace("\u0111", "d");

        foreach (var (pattern, replacement) in AbbreviationRules)
        {
            normalized = pattern.Replace(normalized, replacement);
        }

        normalized = NonAlphaNumericRegex().Replace(normalized, " ");
        normalized = ExtraSpaceRegex().Replace(normalized, " ").Trim();

        return normalized;
    }

    public static double CalculateSimilarity(string? left, string? right)
    {
        var normalizedLeft = NormalizeBusinessName(left);
        var normalizedRight = NormalizeBusinessName(right);

        if (string.IsNullOrWhiteSpace(normalizedLeft) || string.IsNullOrWhiteSpace(normalizedRight))
        {
            return 0;
        }

        if (normalizedLeft.Contains(normalizedRight, StringComparison.Ordinal) ||
            normalizedRight.Contains(normalizedLeft, StringComparison.Ordinal))
        {
            var shorter = Math.Min(normalizedLeft.Length, normalizedRight.Length);
            var longer = Math.Max(normalizedLeft.Length, normalizedRight.Length);
            return longer == 0 ? 0 : Math.Max(0.85, (double)shorter / longer);
        }

        var distance = LevenshteinDistance(normalizedLeft, normalizedRight);
        var maxLength = Math.Max(normalizedLeft.Length, normalizedRight.Length);
        return maxLength == 0 ? 1 : 1 - ((double)distance / maxLength);
    }

    private static string RemoveVietnameseDiacritics(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static int LevenshteinDistance(string left, string right)
    {
        var costs = new int[right.Length + 1];
        for (var j = 0; j <= right.Length; j++)
        {
            costs[j] = j;
        }

        for (var i = 1; i <= left.Length; i++)
        {
            var previousDiagonal = costs[0];
            costs[0] = i;

            for (var j = 1; j <= right.Length; j++)
            {
                var temporary = costs[j];
                var substitutionCost = left[i - 1] == right[j - 1] ? 0 : 1;

                costs[j] = Math.Min(
                    Math.Min(costs[j] + 1, costs[j - 1] + 1),
                    previousDiagonal + substitutionCost);

                previousDiagonal = temporary;
            }
        }

        return costs[right.Length];
    }

    private static Regex WordRegex(string word) => new($@"\b{Regex.Escape(word)}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    [GeneratedRegex("[^a-z0-9\\s]", RegexOptions.Compiled)]
    private static partial Regex NonAlphaNumericRegex();

    [GeneratedRegex("\\s+", RegexOptions.Compiled)]
    private static partial Regex ExtraSpaceRegex();
}
