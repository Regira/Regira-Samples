using Regira.Entities.Models;

namespace ShoppingList.API.Entities.ShoppingListItems;

/// <summary>
/// Filter options for <see cref="ShoppingListItem"/>. Supports filtering items on a list by active
/// state, by the article's category, and by a text search (<c>Q</c>) over the article content.
/// </summary>
public record ShoppingListItemSearchObject : SearchObject
{
    /// <summary>Return items belonging to any of these lists.</summary>
    public ICollection<int>? ShoppingListId { get; set; }

    /// <summary>Return items referencing any of these articles.</summary>
    public ICollection<int>? ArticleId { get; set; }

    /// <summary>Filter by active/inactive state.</summary>
    public bool? IsActive { get; set; }

    /// <summary>Return items whose article belongs to any of these categories.</summary>
    public ICollection<int>? CategoryId { get; set; }
}
