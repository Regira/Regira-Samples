using System.ComponentModel.DataAnnotations;
using Blog.Api.Entities.Categories;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Blog.Api.Entities.BlogPosts;

public class BlogPost : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasNormalizedContent
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = null!;

    [Required, MaxLength(220)]
    public string Slug { get; set; } = null!;

    [Required, MaxLength(500)]
    public string Summary { get; set; } = null!;

    [Required]
    public string Content { get; set; } = null!;

    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<BlogPostTag>? Tags { get; set; }

    [MaxLength(2048), Normalized(SourceProperties = new[] { nameof(Title), nameof(Summary) })]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
