using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Blog.Api.Entities.Posts;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Blog.Api.Entities.Categories;

// Reference data behind BlogPost.CategoryId: deliberately NOT IArchivable (real DELETE, Restrict -> 409 while in use)
public class Category : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasSlug, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(128)] public string? Slug { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    /// <summary>Accent colour (#rrggbb) used by the SPA for badges and card accents.</summary>
    [MaxLength(16)] public string? Color { get; set; }
    public int SortOrder { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    /// <summary>Back-reference used by query filters only - never included, not on the DTOs.</summary>
    public ICollection<BlogPost>? Posts { get; set; }

    [NotMapped] public int? PostCount { get; set; }          // filled by CategoryProcessor
    [NotMapped] public int? PublishedPostCount { get; set; } // filled by CategoryProcessor
}
