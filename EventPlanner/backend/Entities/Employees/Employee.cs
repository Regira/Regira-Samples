using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace EventPlanner.Api.Entities.Employees;

public class Employee : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!; // full name
    [Required, MaxLength(256)] public string Email { get; set; } = null!;
    [MaxLength(128)] public string? Department { get; set; }
    [MaxLength(128)] public string? JobTitle { get; set; }
    [MaxLength(1024)] public string? AvatarUrl { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Email), nameof(Department)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
