using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace AssetHub.Api.Entities.Categories;

// Reference data behind a required FK: deliberately NOT IArchivable (real DELETE + Restrict -> 409 while in use)
public class Category : IEntityWithSerial, IHasTitle, IHasDescription, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string? Title { get; set; }
    [MaxLength(512)] public string? Description { get; set; }
    /// <summary>Icon hint for the SPA (e.g. "laptop", "monitor", "phone")</summary>
    [MaxLength(32)] public string? Icon { get; set; }
    /// <summary>Depreciation period in months (informational)</summary>
    public int? LifespanMonths { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public record CategorySearchObject : SearchObject;

public class CategoryDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int? LifespanMonths { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class CategoryInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string? Title { get; set; }
    [MaxLength(512)] public string? Description { get; set; }
    [MaxLength(32)] public string? Icon { get; set; }
    [Range(0, 600)] public int? LifespanMonths { get; set; }
}
