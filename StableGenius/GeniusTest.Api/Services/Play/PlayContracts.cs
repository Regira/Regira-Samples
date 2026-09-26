using System.ComponentModel.DataAnnotations;
using GeniusTest.Api.Entities.Questions;
using GeniusTest.Api.Entities.Reactions;

namespace GeniusTest.Api.Services.Play;

public class StartGameRequest
{
    [MaxLength(64)] public string? PlayerName { get; set; }
    [MaxLength(32)] public string? Honorific { get; set; }
}

public class AnswerRequest
{
    public int QuestionId { get; set; }
    public int? OptionId { get; set; }
    public double? Number { get; set; }
    public bool Skipped { get; set; }
}

/// <summary>A question as the player sees it: no hint of which option is the official one.</summary>
public record PlayQuestion
{
    public int Id { get; init; }
    public int Index { get; init; }
    public string? Title { get; init; }
    public string? Emoji { get; init; }
    public QuestionType Type { get; init; }
    public QuestionCategory Category { get; init; }
    public double? MinValue { get; init; }
    public double? MaxValue { get; init; }
    public string? Unit { get; init; }
    public IReadOnlyList<PlayOption> Options { get; init; } = [];
}

/// <summary>Unreachable = rendered, but it dodges every attempt (and the API refuses it anyway).</summary>
public record PlayOption(int Id, string? Text, bool Unreachable);

public record GameState
{
    public Guid Key { get; init; }
    public string? PlayerName { get; init; }
    public string? Honorific { get; init; }
    public long Score { get; init; }
    public int Level { get; init; }
    public string? Title { get; init; }
    public bool IsFinished { get; init; }
    public IReadOnlyList<PlayQuestion> Questions { get; init; } = [];
    public IReadOnlyList<AnswerResult> Answers { get; init; } = [];
}

public record AnswerResult
{
    public int QuestionId { get; init; }
    public int Index { get; init; }
    public string? AnswerText { get; init; }
    public SpinStrategy Strategy { get; init; }
    public string? Headline { get; init; }
    public string? Body { get; init; }
    public string? Closer { get; init; }
    public int Points { get; init; }
    public string? BonusLabel { get; init; }
    public int Rarity { get; init; }
    public string? Effect { get; init; }
    public bool Legendary { get; init; }
    public long Score { get; init; }
    public int Level { get; init; }
    public string? PreviousTitle { get; init; }
    public string? Title { get; init; }
    public bool Revealed { get; init; }
    public bool IsLast { get; init; }
}

public record RevealResult
{
    public int QuestionId { get; init; }
    /// <summary>The "official" answer, according to so-called experts.</summary>
    public string? OfficialAnswer { get; init; }
    public string? Note { get; init; }
    public bool YouWereRight { get; init; }
    public string? Praise { get; init; }
    public int Points { get; init; }
    public long Score { get; init; }
    public int FactsViewed { get; init; }
}

public record LeaderboardEntry(int Rank, string Name, string? Title, long Score, bool IsYou);

public record FinishResult
{
    public Guid Key { get; init; }
    public string? PlayerName { get; init; }
    public string? Honorific { get; init; }
    public string? Title { get; init; }
    public long Score { get; init; }
    public int Iq { get; init; }
    public string? IqNote { get; init; }
    public string? CertificateNo { get; init; }
    public DateTime Finished { get; init; }
    public int Correct { get; init; }
    public int Original { get; init; }
    public int Skipped { get; init; }
    public int FactsViewed { get; init; }
    public IReadOnlyList<LeaderboardEntry> Leaderboard { get; init; } = [];
}
