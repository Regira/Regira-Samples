using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace HelpDesk.Api.Entities.Statuses;

/// <summary>A workflow state; the Kanban board renders one column per status, ordered by SortOrder.</summary>
public class Status : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(32)] public string? Title { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    [MaxLength(16)] public string? Color { get; set; }
    public int SortOrder { get; set; }
    /// <summary>The status a new ticket gets.</summary>
    public bool IsDefault { get; set; }
    /// <summary>Tickets in this status count as done (resolved/closed): ClosedAt is stamped, they leave the open queues.</summary>
    public bool IsClosed { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class StatusDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
    public bool IsClosed { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class StatusInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(32)] public string? Title { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    [MaxLength(16)] public string? Color { get; set; }
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
    public bool IsClosed { get; set; }
}
