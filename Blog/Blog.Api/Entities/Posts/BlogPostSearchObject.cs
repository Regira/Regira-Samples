using Regira.Entities.Models;

namespace Blog.Api.Entities.Posts;

public record BlogPostSearchObject : SearchObject
{
    public ICollection<int>? CategoryId { get; set; }
    public ICollection<int>? TagId { get; set; }
    public string? CategorySlug { get; set; }
    public string? TagSlug { get; set; }
    public string? Slug { get; set; }
    public string? Author { get; set; }
    /// <summary>true = publicly visible (published and PublishedAt passed); false = drafts and scheduled posts.</summary>
    public bool? IsPublished { get; set; }
    public PostStatus? Status { get; set; }
    public bool? IsFeatured { get; set; }
    public DateTime? MinPublishedAt { get; set; }
    public DateTime? MaxPublishedAt { get; set; }
    public int? Year { get; set; }
}

/// <summary>Derived state, used as a filter: Draft = not published, Scheduled = published with a future date.</summary>
public enum PostStatus
{
    Draft,
    Scheduled,
    Published
}

public enum BlogPostSortBy
{
    Default = 0,
    Newest,
    Oldest,
    Title,
    TitleDesc,
    ReadingTime,
    ReadingTimeDesc,
    LastModified,
    Created
}

[Flags]
public enum BlogPostIncludes
{
    Default = 0,
    Tags = 1 << 0,
    All = Tags
}
