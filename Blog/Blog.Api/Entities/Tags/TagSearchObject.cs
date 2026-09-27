using Regira.Entities.Models;

namespace Blog.Api.Entities.Tags;

public record TagSearchObject : SearchObject
{
    public string? Slug { get; set; }
    /// <summary>Only tags used by at least one publicly visible post.</summary>
    public bool? HasPublishedPosts { get; set; }
}
