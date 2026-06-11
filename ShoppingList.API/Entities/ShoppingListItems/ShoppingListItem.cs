using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using ShoppingList.API.Entities.Articles;

namespace ShoppingList.API.Entities.ShoppingListItems;

/// <summary>
/// An article placed on a <see cref="ShoppingLists.ShoppingList"/>. The shopper can activate or
/// deactivate it (<see cref="IsActive"/>) without removing it from the list.
/// </summary>
public class ShoppingListItem : IEntityWithSerial, IHasTimestamps
{
    public int Id { get; set; }

    public int ShoppingListId { get; set; }
    public ShoppingLists.ShoppingList? ShoppingList { get; set; }

    public int ArticleId { get; set; }
    public Article? Article { get; set; }

    /// <summary>Whether the article is currently active (to be bought) on the list.</summary>
    public bool IsActive { get; set; } = true;

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    [MaxLength(256)]
    public string? Note { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
