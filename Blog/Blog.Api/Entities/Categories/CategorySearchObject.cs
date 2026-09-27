using Regira.Entities.Models;

namespace Blog.Api.Entities.Categories;

public record CategorySearchObject : SearchObject
{
    public string? Slug { get; set; }
    /// <summary>Only categories that have at least one publicly visible post.</summary>
    public bool? HasPublishedPosts { get; set; }
}
