using System.Globalization;
using GeniusTest.Api.Entities.Questions;
using GeniusTest.Api.Entities.Reactions;

namespace GeniusTest.Api.Services.Spin;

public enum AnswerDirection { None = 0, Higher, Lower }

/// <summary>Everything the engine may know about an answer. Deliberately NOT the official answer.</summary>
public record SpinContext
{
    public required QuestionType QuestionType { get; init; }
    public QuestionCategory Category { get; init; }
    public required string AnswerText { get; init; }
    public bool Skipped { get; init; }
    public bool? IsCorrect { get; init; }
    public AnswerDirection Direction { get; init; }
    public int Rarity { get; init; }
    /// <summary>0-based position in the game; drives praise inflation.</summary>
    public int Index { get; init; }
    public string? CustomReaction { get; init; }
    /// <summary>A correct answer whose PraiseAs name must be confirmed in the praise.</summary>
    public bool Confirms { get; init; }
    public IReadOnlyCollection<SpinStrategy> RecentStrategies { get; init; } = [];
    public IReadOnlySet<int> UsedReactionIds { get; init; } = new HashSet<int>();
    public required string PlayerName { get; init; }
    public string? Honorific { get; init; }
    public required string NewTitle { get; init; }
}

public record SpinResult
{
    public required SpinStrategy Strategy { get; init; }
    public required string Headline { get; init; }
    public required string Body { get; init; }
    public required string Closer { get; init; }
    public required int Points { get; init; }
    public required string BonusLabel { get; init; }
    public required string Effect { get; init; }
    public bool Legendary { get; init; }
    public IReadOnlyList<int> ReactionIds { get; init; } = [];
    public string FullText => $"{Headline} {Body} {Closer}";
}

/// <summary>
/// Turns any answer into praise. Picks a strategy that fits the answer, then an opener/spin/closer
/// the player hasn't seen yet this game. Never has access to the official answer, so it cannot spoil it.
/// </summary>
public class SpinDoctor(Random rng, GameContent content)
{
    public const double LegendaryChance = 0.03;
    public const double CustomReactionChance = 0.65;

    private static readonly string[] AnyEffects = ["confetti", "coins", "crown", "eagle"];

    public SpinResult Spin(SpinContext ctx, IReadOnlyCollection<Reaction> templates)
    {
        var strategy = PickStrategy(ctx);
        var (points, bonusKey) = Score(ctx, strategy);
        var used = new List<int>();

        var body = strategy == SpinStrategy.Custom
            ? ctx.CustomReaction!
            : Pick(templates, ReactionPart.Spin, strategy, ctx.UsedReactionIds, used, specificOnly: true)
              ?? Pick(templates, ReactionPart.Spin, SpinStrategy.JealousExperts, ctx.UsedReactionIds, used, specificOnly: true)
              ?? content.Text(GameTextKeys.FallbackSpin);
        var headline = Pick(templates, ReactionPart.Opener, strategy, ctx.UsedReactionIds, used) ?? content.Text(GameTextKeys.FallbackOpener);
        var closer = Pick(templates, ReactionPart.Closer, strategy, ctx.UsedReactionIds, used) ?? content.Text(GameTextKeys.FallbackCloser);

        var values = Placeholders(ctx, points);
        return new SpinResult
        {
            Strategy = strategy,
            Headline = Fill(headline, values),
            Body = Fill(body, values),
            Closer = Fill(closer, values),
            Points = points,
            BonusLabel = content.Text(bonusKey),
            Effect = PickEffect(strategy),
            Legendary = strategy == SpinStrategy.Legendary,
            ReactionIds = used
        };
    }

    public SpinStrategy PickStrategy(SpinContext ctx)
    {
        if (ctx.Skipped) return SpinStrategy.PowerMove;
        if (ctx.QuestionType == QuestionType.Rating) return SpinStrategy.TooModest;
        // a correct option's hand-written praise is always used (a wrong option's only most of the time, below)
        if (ctx.IsCorrect == true)
            return !string.IsNullOrWhiteSpace(ctx.CustomReaction) ? SpinStrategy.Custom
                : ctx.Confirms ? SpinStrategy.Confirmed : SpinStrategy.Correct;
        if (ctx.Index > 0 && rng.NextDouble() < LegendaryChance) return SpinStrategy.Legendary;
        if (!string.IsNullOrWhiteSpace(ctx.CustomReaction) && rng.NextDouble() < CustomReactionChance) return SpinStrategy.Custom;

        var candidates = new List<(SpinStrategy Strategy, int Weight)>();
        if (ctx.Rarity <= 25) candidates.Add((SpinStrategy.TopPercent, 3));
        if (ctx.Rarity >= 40) candidates.Add((SpinStrategy.Landslide, 3));
        if (ctx.Direction == AnswerDirection.Higher) candidates.Add((SpinStrategy.ThinkBig, 4));
        if (ctx.Direction == AnswerDirection.Lower) candidates.Add((SpinStrategy.LeanAndEfficient, 4));
        if (ctx.Category == QuestionCategory.Science) candidates.Add((SpinStrategy.AheadOfScience, 3));
        if (ctx.QuestionType == QuestionType.Choice) candidates.Add((SpinStrategy.BetterThanReality, 2));
        candidates.Add((SpinStrategy.Rigged, 1));
        candidates.Add((SpinStrategy.JealousExperts, 1));

        // Variety: avoid the strategies of the last couple of answers while alternatives remain.
        var fresh = candidates.Where(c => !ctx.RecentStrategies.Contains(c.Strategy)).ToList();
        return WeightedPick(fresh.Count > 0 ? fresh : candidates);
    }

    /// <summary>Points only ever go up - and a creative answer earns more than a correct one.</summary>
    public static (int Points, string BonusKey) Score(SpinContext ctx, SpinStrategy strategy)
    {
        var (basePoints, bonusKey) = strategy switch
        {
            SpinStrategy.Legendary => (2500, GameTextKeys.BonusLegendary),
            SpinStrategy.Correct or SpinStrategy.Confirmed => (100, GameTextKeys.BonusCorrect),
            SpinStrategy.PowerMove => (500, GameTextKeys.BonusPowerMove),
            SpinStrategy.TooModest => (300, GameTextKeys.BonusTooModest),
            _ => (250, GameTextKeys.BonusOriginal)
        };
        var multiplier = 1 + ctx.Index * 0.5; // praise inflation
        return ((int)Math.Round(basePoints * multiplier), bonusKey);
    }

    public string? Pick(IReadOnlyCollection<Reaction> templates, ReactionPart part, SpinStrategy strategy,
        IReadOnlySet<int> alreadyUsed, List<int> usedNow, bool specificOnly = false)
    {
        var pool = templates.Where(t => t.IsActive && t.Part == part && !string.IsNullOrWhiteSpace(t.Text)).ToList();
        var specific = pool.Where(t => t.Strategy == strategy).ToList();
        var generic = specificOnly ? [] : pool.Where(t => t.Strategy == SpinStrategy.Any).ToList();
        // Openers/closers: half of the time the strategy-specific flavour, when it exists.
        var candidates = specific.Count > 0 && (specificOnly || generic.Count == 0 || rng.NextDouble() < 0.5)
            ? specific
            : generic;
        if (candidates.Count == 0) return null;

        var unseen = candidates.Where(t => !alreadyUsed.Contains(t.Id) && !usedNow.Contains(t.Id)).ToList();
        var pick = (unseen.Count > 0 ? unseen : candidates)[rng.Next(unseen.Count > 0 ? unseen.Count : candidates.Count)];
        usedNow.Add(pick.Id);
        return pick.Text;
    }

    public string PickEffect(SpinStrategy strategy) => strategy switch
    {
        SpinStrategy.Legendary => "legendary",
        SpinStrategy.Correct or SpinStrategy.Confirmed => "fireworks",
        SpinStrategy.PowerMove or SpinStrategy.TooModest => "crown",
        SpinStrategy.TopPercent => "trophy",
        SpinStrategy.ThinkBig => "coins",
        _ => AnyEffects[rng.Next(AnyEffects.Length)]
    };

    public Dictionary<string, string> Placeholders(SpinContext ctx, int points, int? iq = null) => new()
    {
        ["{answer}"] = ctx.AnswerText,
        ["{pct}"] = ctx.Rarity.ToString(CultureInfo.InvariantCulture),
        ["{name}"] = ctx.PlayerName,
        ["{honorific}"] = string.IsNullOrWhiteSpace(ctx.Honorific) ? content.Text(GameTextKeys.DefaultHonorific) : ctx.Honorific,
        ["{title}"] = ctx.NewTitle,
        ["{years}"] = (rng.Next(5, 51) * 10).ToString(CultureInfo.InvariantCulture),
        ["{points}"] = points.ToString("N0", CultureInfo.InvariantCulture),
        ["{iq}"] = (iq ?? 0).ToString(CultureInfo.InvariantCulture)
    };

    public static string Fill(string template, IReadOnlyDictionary<string, string> values)
        => values.Aggregate(template, (text, kv) => text.Replace(kv.Key, kv.Value, StringComparison.OrdinalIgnoreCase));

    private SpinStrategy WeightedPick(IReadOnlyList<(SpinStrategy Strategy, int Weight)> candidates)
    {
        var roll = rng.Next(candidates.Sum(c => c.Weight));
        foreach (var (strategy, weight) in candidates)
        {
            if (roll < weight) return strategy;
            roll -= weight;
        }
        return candidates[^1].Strategy;
    }
}
