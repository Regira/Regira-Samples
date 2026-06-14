using Regira.Entities.Models;

namespace ShoppingListApi.Entities.Articles;

/// <summary>
/// Filter options for articles. <c>Q</c> (inherited) performs a normalized full-text search over
/// title + description + brand via the global normalized-content filter.
/// </summary>
public record ArticleSearchObject : SearchObject
{
    /// <summary>Return articles belonging to any of these categories.</summary>
    public ICollection<int>? CategoryId { get; set; }

    /// <summary>Return articles whose brand contains this text.</summary>
    public string? Brand { get; set; }
}
