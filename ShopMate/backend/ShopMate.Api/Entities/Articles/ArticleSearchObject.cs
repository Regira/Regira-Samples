using Regira.Entities.Models;

namespace ShopMate.Api.Entities.Articles;

public record ArticleSearchObject : SearchObject
{
    public ICollection<int>? ShoppingListId { get; set; }
    public ICollection<int>? ShopperId { get; set; }
    /// <summary>
    /// Matches articles tagged with ANY of these categories. The SPA expands a picked category to its
    /// descendants client-side (the hierarchy is already loaded there) and sends the whole set.
    /// </summary>
    public ICollection<int>? CategoryId { get; set; }
    public bool? IsActive { get; set; }
}

public enum ArticleSortBy
{
    Default = 0,
    SortOrder,
    ActiveFirst,
    Title,
    TitleDesc,
    Created,
    CreatedDesc
}

[Flags]
public enum ArticleIncludes
{
    Default = 0,
    Categories = 1 << 0,
    ShoppingList = 1 << 1,
    All = Categories | ShoppingList
}
