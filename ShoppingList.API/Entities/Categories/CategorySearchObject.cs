using Regira.Entities.Models;

namespace ShoppingListApi.Entities.Categories;

/// <summary>
/// Filter options for categories. <c>Q</c> (inherited) performs a normalized full-text search
/// over title + description via the global normalized-content filter.
/// </summary>
public record CategorySearchObject : SearchObject
{
    /// <summary>Return categories that have any of these categories as a parent.</summary>
    public ICollection<int>? ParentId { get; set; }

    /// <summary>Return categories that have any of these categories as a child.</summary>
    public ICollection<int>? ChildId { get; set; }

    /// <summary>When set, return only root categories (no parents) or only non-root categories.</summary>
    public bool? IsRoot { get; set; }
}
