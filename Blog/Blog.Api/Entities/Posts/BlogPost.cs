using System.ComponentModel.DataAnnotations;
using Blog.Api.Entities.Categories;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Blog.Api.Entities.Posts;

public class BlogPost : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasSlug, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(160)] public string Title { get; set; } = null!;
    [MaxLength(128)] public string? Slug { get; set; }
    /// <summary>Short teaser shown on article cards.</summary>
    [MaxLength(512)] public string? Summary { get; set; }
    /// <summary>Article body in a small Markdown subset (## / ### headings, paragraphs, "- " lists, "> " quotes).</summary>
    public string? Content { get; set; }
    [MaxLength(512)] public string? CoverImageUrl { get; set; }
    [MaxLength(96)] public string? AuthorName { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    /// <summary>Editorial switch. A post is publicly visible when IsPublished and PublishedAt has passed.</summary>
    public bool IsPublished { get; set; }
    /// <summary>Publication moment (UTC). A future value schedules the post.</summary>
    public DateTime? PublishedAt { get; set; }
    public bool IsFeatured { get; set; }

    /// <summary>Derived from Content on every save (BlogPostPrepper) - not client-writable.</summary>
    public int ReadingTimeMinutes { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Summary), nameof(AuthorName)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<BlogPostTag>? Tags { get; set; }
}
