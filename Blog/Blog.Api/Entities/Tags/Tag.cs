using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Blog.Api.Entities.Posts;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Blog.Api.Entities.Tags;

public class Tag : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasSlug, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(48)] public string Title { get; set; } = null!;
    [MaxLength(64)] public string? Slug { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    /// <summary>Back-reference used by query filters only - never included, not on the DTOs.</summary>
    public ICollection<BlogPostTag>? PostTags { get; set; }

    [NotMapped] public int? PostCount { get; set; }          // filled by TagProcessor
    [NotMapped] public int? PublishedPostCount { get; set; } // filled by TagProcessor
}
