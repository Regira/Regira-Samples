using Blog.Api.Entities.Categories;
using Blog.Api.Entities.Tags;

namespace Blog.Api.Entities.BlogPosts;

public class BlogPostDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Summary { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? CoverImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int CategoryId { get; set; }
    public CategoryDto? Category { get; set; }
    public ICollection<BlogPostTagDto>? Tags { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class BlogPostTagDto
{
    public int Id { get; set; }
    public int BlogPostId { get; set; }
    public int TagId { get; set; }
    public TagDto? Tag { get; set; }
}
