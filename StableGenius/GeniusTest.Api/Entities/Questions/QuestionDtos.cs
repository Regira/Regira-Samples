using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models;

namespace GeniusTest.Api.Entities.Questions;

public record QuestionSearchObject : SearchObject
{
    public ICollection<QuestionType>? Type { get; set; }
    public ICollection<QuestionCategory>? Category { get; set; }
    public bool? IsActive { get; set; }
}

public class QuestionDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Emoji { get; set; }
    public QuestionType Type { get; set; }
    public QuestionCategory Category { get; set; }
    public double? CorrectNumber { get; set; }
    public double? Tolerance { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public string? Unit { get; set; }
    public string? RevealNote { get; set; }
    public bool IsActive { get; set; }
    public bool AlwaysAsked { get; set; }
    public ICollection<QuestionOptionDto>? Options { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class QuestionOptionDto
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string? Text { get; set; }
    public bool IsCorrect { get; set; }
    public string? CustomReaction { get; set; }
    public bool IsUnreachable { get; set; }
    public string? PraiseAs { get; set; }
    public int SortOrder { get; set; }
}

public class QuestionInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(256)] public string? Title { get; set; }
    [MaxLength(16)] public string? Emoji { get; set; }
    public QuestionType Type { get; set; }
    public QuestionCategory Category { get; set; }
    public double? CorrectNumber { get; set; }
    public double? Tolerance { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    [MaxLength(32)] public string? Unit { get; set; }
    [MaxLength(512)] public string? RevealNote { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AlwaysAsked { get; set; }
    // null = not sent (options untouched); [] = delete all
    public ICollection<QuestionOptionInputDto>? Options { get; set; }
}

public class QuestionOptionInputDto
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    [Required, MaxLength(128)] public string? Text { get; set; }
    public bool IsCorrect { get; set; }
    [MaxLength(512)] public string? CustomReaction { get; set; }
    public bool IsUnreachable { get; set; }
    [MaxLength(128)] public string? PraiseAs { get; set; }
    public int SortOrder { get; set; }
}
