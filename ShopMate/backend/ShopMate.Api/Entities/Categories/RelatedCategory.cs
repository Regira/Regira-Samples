using Regira.Entities.Models.Abstractions;

namespace ShopMate.Api.Entities.Categories;

/// <summary>
/// Self-referencing join entity: a Category can have several parents and several children
/// (a many-to-many hierarchy, not a single-parent tree).
/// </summary>
public class RelatedCategory : IEntityWithSerial
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public Category Parent { get; set; } = null!;
    public int ChildId { get; set; }
    public Category Child { get; set; } = null!;
}
