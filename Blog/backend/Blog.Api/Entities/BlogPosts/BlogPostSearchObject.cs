using Regira.Entities.Models;

namespace Blog.Api.Entities.BlogPosts;

public record BlogPostSearchObject : SearchObject
{
    public ICollection<int>? CategoryId { get; set; }
    public ICollection<int>? TagId { get; set; }
    public bool? IsPublished { get; set; }
    public DateTime? MinPublishedAt { get; set; }
    public DateTime? MaxPublishedAt { get; set; }
    public string? Slug { get; set; }
}
