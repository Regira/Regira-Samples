using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Blog.Api.Entities.Categories;

public class Category : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasNormalizedContent
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Title { get; set; } = null!;

    [Required, MaxLength(120)]
    public string Slug { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = new[] { nameof(Title), nameof(Description) })]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    // filled by CategoryProcessor
    [NotMapped]
    public int? PostCount { get; set; }
}
