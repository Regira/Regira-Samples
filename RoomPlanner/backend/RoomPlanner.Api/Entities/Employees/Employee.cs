using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace RoomPlanner.Api.Entities.Employees;

public class Employee : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasNormalizedContent
{
    public int Id { get; set; }

    /// <summary>Full name.</summary>
    [Required, MaxLength(128)]
    public string Title { get; set; } = null!;

    [Required, MaxLength(256)]
    public string Email { get; set; } = null!;

    [MaxLength(128)]
    public string? Department { get; set; }

    [MaxLength(128)]
    public string? JobTitle { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Email), nameof(Department)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
