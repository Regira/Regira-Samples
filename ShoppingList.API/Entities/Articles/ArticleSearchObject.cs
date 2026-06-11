using Regira.Entities.Models;

namespace ShoppingList.API.Entities.Articles;

/// <summary>
/// Filter options for <see cref="Article"/>. <c>Q</c> (inherited) performs a normalized full-text
/// search over title, description and brand.
/// </summary>
public record ArticleSearchObject : SearchObject
{
    /// <summary>Return articles that belong to any of these categories.</summary>
    public ICollection<int>? CategoryId { get; set; }

    /// <summary>Return articles of any of these brands.</summary>
    public ICollection<string>? Brand { get; set; }
}
