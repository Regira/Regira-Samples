using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace EventPlanner.Api.Entities.Speakers;

public class Speaker : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!; // full name
    [MaxLength(2048)] public string? Description { get; set; } // bio
    [MaxLength(128)] public string? JobTitle { get; set; }
    [MaxLength(128)] public string? Company { get; set; }
    [MaxLength(256)] public string? Email { get; set; }
    [MaxLength(1024)] public string? PhotoUrl { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Company), nameof(JobTitle)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
