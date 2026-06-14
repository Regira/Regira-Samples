using Regira.Entities.Models.Abstractions;

namespace ShoppingListApi.Entities.Categories;

/// <summary>
/// Self-referencing join entity that links a parent <see cref="Category"/> to a child
/// <see cref="Category"/>. Owned and managed through the <see cref="Category"/> service via
/// <c>Related()</c>; it has no standalone entity-service registration.
/// </summary>
public class RelatedCategory : IEntityWithSerial
{
    public int Id { get; set; }

    public int ChildId { get; set; }
    public Category Child { get; set; } = null!;

    public int ParentId { get; set; }
    public Category Parent { get; set; } = null!;
}
