using System.ComponentModel.DataAnnotations;
using GeniusTest.Api.Entities.Reactions;
using Regira.Entities.Attributes;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace GeniusTest.Api.Entities.Games;

/// <summary>
/// One play-through. Everything but the player's name is written by the play service (raw DbContext),
/// so the admin API may rename a player but never touch the score.
/// </summary>
public class Game : IEntityWithSerial, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    // Unguessable handle the player uses in /play/{key}; minted on create.
    public Guid Key { get; set; }
    [Required, MaxLength(64)] public string? PlayerName { get; set; }
    [MaxLength(32)] public string? Honorific { get; set; }

    [ServerOwned] public long Score { get; set; }
    [ServerOwned] public int Level { get; set; }
    [ServerOwned, MaxLength(64)] public string? GeniusTitle { get; set; }
    [ServerOwned] public int? Iq { get; set; }
    [ServerOwned] public int FactsViewed { get; set; }
    // Comma-separated question ids in play order.
    [ServerOwned, MaxLength(256)] public string? QuestionIds { get; set; }
    // The game's own copy of its questions (JSON of QuestionSnapshot), so a bank edit or reset never reaches it.
    [ServerOwned] public string? QuestionsJson { get; set; }
    [ServerOwned] public DateTime? Finished { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<GameAnswer>? Answers { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(PlayerName), nameof(GeniusTitle)])]
    public string? NormalizedContent { get; set; }
}

/// <summary>One answered question, with the praise it earned (a snapshot: questions may change later).</summary>
public class GameAnswer : IEntityWithSerial
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game? Game { get; set; }
    public int SortOrder { get; set; }

    // Id of the question in the game's snapshot - deliberately not a foreign key into the (resettable) bank.
    public int? QuestionId { get; set; }
    [MaxLength(256)] public string? QuestionText { get; set; }

    public int? OptionId { get; set; }
    public double? NumberValue { get; set; }
    [MaxLength(128)] public string? AnswerText { get; set; }
    public bool Skipped { get; set; }
    // null for questions without an official answer (ratings)
    public bool? IsCorrect { get; set; }

    public SpinStrategy Strategy { get; set; }
    public int Rarity { get; set; }
    public int Points { get; set; }
    [MaxLength(1024)] public string? ReactionText { get; set; }
    // Template ids used, so a game never repeats itself.
    [MaxLength(64)] public string? ReactionIds { get; set; }
    [MaxLength(32)] public string? Effect { get; set; }
    public bool Revealed { get; set; }
    public DateTime Answered { get; set; }
}
