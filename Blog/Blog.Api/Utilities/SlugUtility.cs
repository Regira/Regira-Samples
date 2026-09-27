using System.Globalization;
using System.Text;

namespace Blog.Api.Utilities;

public static class SlugUtility
{
    public const int MaxLength = 120;

    /// <summary>Lower-case, diacritics stripped, every run of non-alphanumerics collapsed to one dash.</summary>
    public static string Slugify(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        var lastWasDash = true;
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark || c == '\'' || c == '\u2019')
                continue;
            if (char.IsLetterOrDigit(c) && c < 128)
            {
                sb.Append(char.ToLowerInvariant(c));
                lastWasDash = false;
            }
            else if (!lastWasDash)
            {
                sb.Append('-');
                lastWasDash = true;
            }
        }

        var slug = sb.ToString().Trim('-');
        if (slug.Length > MaxLength)
            slug = slug[..MaxLength].TrimEnd('-');
        return slug;
    }
}
