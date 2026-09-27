using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace HelpDesk.Api.Entities.Priorities;

public class Priority : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(32)] public string? Title { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    /// <summary>Higher is more urgent; drives sorting.</summary>
    public int Level { get; set; }
    [MaxLength(16)] public string? Color { get; set; }
    /// <summary>Resolution target in hours; a new ticket's DueDate = Created + TargetHours.</summary>
    [Range(1, 24 * 90)] public int TargetHours { get; set; } = 72;
    public bool IsDefault { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class PriorityDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int Level { get; set; }
    public string? Color { get; set; }
    public int TargetHours { get; set; }
    public bool IsDefault { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class PriorityInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(32)] public string? Title { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    public int Level { get; set; }
    [MaxLength(16)] public string? Color { get; set; }
    [Range(1, 24 * 90)] public int TargetHours { get; set; } = 72;
    public bool IsDefault { get; set; }
}
