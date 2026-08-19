using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Blog.Api.Entities.Tags;

public class Tag : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasNormalizedContent
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Title { get; set; } = null!;

    [Required, MaxLength(60)]
    public string Slug { get; set; } = null!;

    [MaxLength(256), Normalized(SourceProperties = new[] { nameof(Title) })]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
