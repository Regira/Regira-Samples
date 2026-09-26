using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace GeniusTest.Api.Entities.Questions;

public enum QuestionType { Choice = 0, Number = 1, Rating = 2 }

public enum QuestionCategory { General = 0, Geography, Math, Science, History, Language, AboutYou }

public class Question : IEntityWithSerial, IHasTitle, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(256)] public string? Title { get; set; }
    [MaxLength(16)] public string? Emoji { get; set; }
    public QuestionType Type { get; set; }
    public QuestionCategory Category { get; set; }

    // Number questions: the official answer and how far off still counts as correct.
    public double? CorrectNumber { get; set; }
    public double? Tolerance { get; set; }
    // Number / Rating questions: input bounds (a Rating defaults to 1-10).
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    [MaxLength(32)] public string? Unit { get; set; }

    // Shown only after the player insists on seeing the "official" answer.
    [MaxLength(512)] public string? RevealNote { get; set; }
    public bool IsActive { get; set; } = true;
    // Asked in every game, right after the opening self-rating (e.g. the gender question).
    public bool AlwaysAsked { get; set; }

    public ICollection<QuestionOption>? Options { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title)])]
    public string? NormalizedContent { get; set; }
}

public class QuestionOption : IEntityWithSerial, ISortable
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public Question? Question { get; set; }
    [Required, MaxLength(128)] public string? Text { get; set; }
    public bool IsCorrect { get; set; }
    // Optional hand-written praise for this option: always used when the option is correct, most of the time when it isn't.
    [MaxLength(512)] public string? CustomReaction { get; set; }
    // Shown, but it runs away from the cursor and the API refuses it.
    public bool IsUnreachable { get; set; }
    // How the praise refers to this answer ("Male" -> "Alpha Male"); a correct one gets a "Confirmed" praise quoting it.
    [MaxLength(128)] public string? PraiseAs { get; set; }
    public int SortOrder { get; set; }
}
