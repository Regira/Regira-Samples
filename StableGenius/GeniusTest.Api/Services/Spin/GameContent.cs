using System.Text.RegularExpressions;
using GeniusTest.Api.Data.Seeding;

namespace GeniusTest.Api.Services.Spin;

/// <summary>
/// Fixed game content read from CSV at startup: the praise-inflation title ladder (titles.csv), the historical
/// greats that fill up the leaderboard (legends.csv) and every text the API shows on a game screen (game-texts.csv).
/// </summary>
public partial class GameContent(IReadOnlyList<string> ladder, string finalTitle, IReadOnlyList<string> legends, IReadOnlyDictionary<string, string> texts)
{
    /// <summary>One title per answered question: every answer - any answer - earns the next one.</summary>
    public IReadOnlyList<string> Ladder { get; } = ladder;
    /// <summary>The title for finishing the game.</summary>
    public string FinalTitle { get; } = finalTitle;
    public IReadOnlyList<string> Legends { get; } = legends;
    public IReadOnlyDictionary<string, string> Texts { get; } = texts;

    public string TitleFor(int level) => Ladder[Math.Clamp(level, 0, Ladder.Count - 1)];

    /// <summary>A text by key (see <see cref="GameTextKeys"/>) with its {placeholders} filled; the key itself when missing.</summary>
    public string Text(string key, IReadOnlyDictionary<string, object?>? values = null)
    {
        var text = Texts.GetValueOrDefault(key, key);
        return values == null
            ? text
            : Placeholder().Replace(text, m => values.TryGetValue(m.Groups[1].Value, out var v) && v != null ? v.ToString()! : m.Value);
    }

    [GeneratedRegex(@"\{(\w+)\}")]
    private static partial Regex Placeholder();

    public static async Task<GameContent> Load(string contentRoot, CancellationToken token = default)
    {
        var titles = await SeedFiles.Read<TitleRow>(contentRoot, "titles.csv", token);
        var ladder = titles.Where(t => !t.IsFinal).OrderBy(t => t.Level).Select(t => t.Title).ToList();
        var final = titles.FirstOrDefault(t => t.IsFinal)?.Title;
        if (ladder.Count == 0 || final == null)
            throw new InvalidOperationException("titles.csv needs at least one ladder title and one row with IsFinal = true");

        var legends = (await SeedFiles.Read<LegendRow>(contentRoot, "legends.csv", token)).Select(l => l.Name).ToList();
        var texts = (await SeedFiles.Read<GameTextRow>(contentRoot, "game-texts.csv", token))
            .GroupBy(t => t.Key)
            .ToDictionary(g => g.Key, g => g.Last().Text);
        return new GameContent(ladder, final, legends, texts);
    }
}
