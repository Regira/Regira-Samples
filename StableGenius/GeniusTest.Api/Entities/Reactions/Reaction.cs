using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace GeniusTest.Api.Entities.Reactions;

/// <summary>Where a template lands in a reaction: "WOW." (opener) + the spin itself + a closing line.</summary>
public enum ReactionPart { Opener = 0, Spin = 1, Closer = 2 }

/// <summary>How an answer is turned into praise. Any = usable for every strategy (openers/closers).</summary>
public enum SpinStrategy
{
    Any = 0,
    Correct,
    BetterThanReality,
    TopPercent,
    Landslide,
    ThinkBig,
    LeanAndEfficient,
    Rigged,
    JealousExperts,
    AheadOfScience,
    TooModest,
    PowerMove,
    Custom,
    Legendary,
    Reveal,
    Finale,
    // a correct answer with a PraiseAs name: the praise confirms it ("Alpha Male. Confirmed.")
    Confirmed
}

/// <summary>A praise template. Placeholders: {answer} {pct} {name} {honorific} {title} {years} {points} {iq}.</summary>
public class Reaction : IEntityWithSerial, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    public ReactionPart Part { get; set; }
    public SpinStrategy Strategy { get; set; }
    [Required, MaxLength(512)] public string? Text { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Text)])]
    public string? NormalizedContent { get; set; }
}

public record ReactionSearchObject : SearchObject
{
    public ICollection<ReactionPart>? Part { get; set; }
    public ICollection<SpinStrategy>? Strategy { get; set; }
    public bool? IsActive { get; set; }
}

public class ReactionDto
{
    public int Id { get; set; }
    public ReactionPart Part { get; set; }
    public SpinStrategy Strategy { get; set; }
    public string? Text { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class ReactionInputDto
{
    public int Id { get; set; }
    public ReactionPart Part { get; set; }
    public SpinStrategy Strategy { get; set; }
    [Required, MaxLength(512)] public string? Text { get; set; }
    public bool IsActive { get; set; } = true;
}
