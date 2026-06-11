using Regira.Entities.Models;

namespace ShoppingList.API.Entities.ShoppingLists;

/// <summary>Filter options for <see cref="ShoppingList"/>. <c>Q</c> searches the list name.</summary>
public record ShoppingListSearchObject : SearchObject
{
    /// <summary>Return lists belonging to any of these shoppers.</summary>
    public ICollection<int>? ShopperId { get; set; }
}
