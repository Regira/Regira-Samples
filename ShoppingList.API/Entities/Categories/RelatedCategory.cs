using Regira.Entities.Models.Abstractions;

namespace ShoppingList.API.Entities.Categories;

/// <summary>
/// Self-referential join entity connecting a parent <see cref="Category"/> to a child category.
/// Managed as an owned collection of <see cref="Category"/> via <c>e.Related(...)</c>.
/// </summary>
public class RelatedCategory : IEntityWithSerial
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int ParentId { get; set; }
    public Category Child { get; set; } = null!;
    public Category Parent { get; set; } = null!;
}
