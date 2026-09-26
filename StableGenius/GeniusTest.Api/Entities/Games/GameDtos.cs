using System.ComponentModel.DataAnnotations;
using GeniusTest.Api.Entities.Reactions;
using Regira.Entities.Models;

namespace GeniusTest.Api.Entities.Games;

public enum GameSortBy { Default = 0, Created, CreatedDesc, Score, ScoreDesc, PlayerName }

[Flags]
public enum GameIncludes { Default = 0, Answers = 1 << 0, All = Answers }

public record GameSearchObject : SearchObject
{
    public bool? IsFinished { get; set; }
    public long? MinScore { get; set; }
}

public class GameDto
{
    public int Id { get; set; }
    public string? PlayerName { get; set; }
    public string? Honorific { get; set; }
    public long Score { get; set; }
    public int Level { get; set; }
    public string? GeniusTitle { get; set; }
    public int? Iq { get; set; }
    public int FactsViewed { get; set; }
    public DateTime? Finished { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<GameAnswerDto>? Answers { get; set; }
}

public class GameAnswerDto
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int SortOrder { get; set; }
    public int? QuestionId { get; set; }
    public string? QuestionText { get; set; }
    public string? AnswerText { get; set; }
    public bool Skipped { get; set; }
    public bool? IsCorrect { get; set; }
    public SpinStrategy Strategy { get; set; }
    public int Rarity { get; set; }
    public int Points { get; set; }
    public string? ReactionText { get; set; }
    public bool Revealed { get; set; }
    public DateTime Answered { get; set; }
}

/// <summary>Admins may fix a player's name. Scores are sacred.</summary>
public class GameInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string? PlayerName { get; set; }
    [MaxLength(32)] public string? Honorific { get; set; }
}
