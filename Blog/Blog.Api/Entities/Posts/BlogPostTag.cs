using Blog.Api.Entities.Tags;
using Regira.Entities.Models.Abstractions;

namespace Blog.Api.Entities.Posts;

// many-to-many join, owned by BlogPost via e.Related() - no own registration, no controller, no budget slot
public class BlogPostTag : IEntityWithSerial
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public BlogPost? Post { get; set; }
    public int TagId { get; set; }
    public Tag? Tag { get; set; }
}
