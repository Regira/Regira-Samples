using Blog.Api.Entities.Tags;
using Regira.Entities.Models.Abstractions;

namespace Blog.Api.Entities.BlogPosts;

// many-to-many join entity between BlogPost and Tag - owned by BlogPost via Related()
public class BlogPostTag : IEntityWithSerial
{
    public int Id { get; set; }

    public int BlogPostId { get; set; }
    public BlogPost BlogPost { get; set; } = null!;

    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
