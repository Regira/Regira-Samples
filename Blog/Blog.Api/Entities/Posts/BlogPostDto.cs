using System.ComponentModel.DataAnnotations;
using Blog.Api.Entities.Categories;
using Blog.Api.Entities.Tags;

namespace Blog.Api.Entities.Posts;

public class BlogPostDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Slug { get; set; }
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? AuthorName { get; set; }
    public int? CategoryId { get; set; }
    public CategoryDto? Category { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool IsFeatured { get; set; }
    public int ReadingTimeMinutes { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<BlogPostTagDto>? Tags { get; set; }
}

public class BlogPostTagDto
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int TagId { get; set; }
    public TagDto? Tag { get; set; }
}

public class BlogPostInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(160)] public string Title { get; set; } = null!;
    /// <summary>Optional - generated from the title when left empty.</summary>
    [MaxLength(128)] public string? Slug { get; set; }
    [MaxLength(512)] public string? Summary { get; set; }
    public string? Content { get; set; }
    [MaxLength(512)] public string? CoverImageUrl { get; set; }
    [MaxLength(96)] public string? AuthorName { get; set; }
    public int? CategoryId { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool IsFeatured { get; set; }
    // nullable + uninitialized: an omitted collection leaves the tags untouched, [] clears them
    public ICollection<BlogPostTagInputDto>? Tags { get; set; }
}

public class BlogPostTagInputDto
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int TagId { get; set; }
}
