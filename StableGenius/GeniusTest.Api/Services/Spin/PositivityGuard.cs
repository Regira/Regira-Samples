using System.Text.RegularExpressions;

namespace GeniusTest.Api.Services.Spin;

/// <summary>
/// The house rule: nothing the player reads may sound like criticism. Every praise text passes through here.
/// </summary>
public static partial class PositivityGuard
{
    public static readonly string[] KnownPlaceholders =
        ["{answer}", "{pct}", "{name}", "{honorific}", "{title}", "{years}", "{points}", "{iq}"];

    [GeneratedRegex(@"\b(wrong|incorrect|mistakes?|fail\w*|sorry|unfortunately|bad|stupid|dumb|idiots?|losers?|errors?|false)\b",
        RegexOptions.IgnoreCase)]
    private static partial Regex NegativeWords();

    [GeneratedRegex(@"\{[a-z]+\}", RegexOptions.IgnoreCase)]
    private static partial Regex Placeholders();

    /// <summary>Negative words found in the text (distinct, lower case).</summary>
    public static IReadOnlyList<string> FindNegativity(string? text)
        => string.IsNullOrWhiteSpace(text)
            ? []
            : NegativeWords().Matches(text).Select(m => m.Value.ToLowerInvariant()).Distinct().ToList();

    /// <summary>Placeholders the spin engine cannot fill - {correct} included: facts are never spoiled.</summary>
    public static IReadOnlyList<string> FindUnknownPlaceholders(string? text)
        => string.IsNullOrWhiteSpace(text)
            ? []
            : Placeholders().Matches(text).Select(m => m.Value.ToLowerInvariant())
                .Where(p => !KnownPlaceholders.Contains(p)).Distinct().ToList();

    /// <summary>A validation message, or null when the text is pure praise.</summary>
    public static string? Validate(string? text)
    {
        var negative = FindNegativity(text);
        if (negative.Count > 0)
            return $"Only positive vibes allowed. Please remove: {string.Join(", ", negative)}.";
        var unknown = FindUnknownPlaceholders(text);
        if (unknown.Count > 0)
            return $"Unknown placeholder(s): {string.Join(", ", unknown)}. Allowed: {string.Join(" ", KnownPlaceholders)}.";
        return null;
    }
}
