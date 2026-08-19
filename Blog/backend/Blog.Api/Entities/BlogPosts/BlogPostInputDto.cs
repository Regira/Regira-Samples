using System.ComponentModel.DataAnnotations;

namespace Blog.Api.Entities.BlogPosts;

public class BlogPostInputDto
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

    // nullable & uninitialized - omitted means "untouched", [] means "delete all"
    public ICollection<BlogPostTagInputDto>? Tags { get; set; }
}

public class BlogPostTagInputDto
{
    public int Id { get; set; }
    public int BlogPostId { get; set; }
    public int TagId { get; set; }
}
