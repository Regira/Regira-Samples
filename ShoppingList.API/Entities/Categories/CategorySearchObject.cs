using Regira.Entities.Models;

namespace ShoppingList.API.Entities.Categories;

/// <summary>
/// Filter options for <see cref="Category"/>. Inherits <c>Id</c>, <c>Ids</c>, <c>Q</c> (text search),
/// timestamp and archive filters from <see cref="SearchObject"/>.
/// </summary>
public record CategorySearchObject : SearchObject
{
    /// <summary>Return categories that have any of these categories as a parent.</summary>
    public ICollection<int>? ParentId { get; set; }

    /// <summary>Return categories that have any of these categories as a child.</summary>
    public ICollection<int>? ChildId { get; set; }

    /// <summary>When true, returns only root categories (no parents); when false, only non-root categories.</summary>
    public bool? IsRoot { get; set; }
}
