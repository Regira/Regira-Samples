using Regira.Entities.Models;

namespace ShoppingListApi.Entities.Lists;

/// <summary>
/// Filter options for shopping lists. <c>Q</c> (inherited) performs a normalized full-text search
/// over the list title via the global normalized-content filter.
/// </summary>
public record ShoppingListSearchObject : SearchObject
{
    /// <summary>Return lists owned by any of these shoppers.</summary>
    public ICollection<int>? ShopperId { get; set; }
}
