using Regira.Entities.Models;

namespace ShoppingListApi.Entities.Shoppers;

/// <summary>
/// Filter options for shoppers. <c>Q</c> (inherited) performs a normalized full-text search over
/// name + email via the global normalized-content filter.
/// </summary>
public record ShopperSearchObject : SearchObject
{
    /// <summary>Return shoppers whose name contains this text.</summary>
    public string? Name { get; set; }
}
