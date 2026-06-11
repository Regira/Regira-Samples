using Regira.Entities.Models.Abstractions;

namespace Webshop.API.Entities.Categories;

public class RelatedCategory : IEntityWithSerial
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int ParentId { get; set; }
    public Category Child { get; set; } = null!;
    public Category Parent { get; set; } = null!;
}
