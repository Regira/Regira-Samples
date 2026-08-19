using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace QCredits.Api.Entities.GroupTrainings;

/// <summary>
/// A company-funded group training. Funded separately from personal QCredit budgets and does not
/// affect any employee's QCredit balance.
/// </summary>
public class GroupTraining : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasNormalizedContent
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Title { get; set; } = null!;

    [MaxLength(2048)]
    public string? Description { get; set; }

    public DateTime TrainingDate { get; set; }

    [MaxLength(128)]
    public string? Location { get; set; }

    [MaxLength(128)]
    public string? Facilitator { get; set; }

    public decimal Cost { get; set; }
    public int MaxParticipants { get; set; }

    [MaxLength(64)]
    public string? Department { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
