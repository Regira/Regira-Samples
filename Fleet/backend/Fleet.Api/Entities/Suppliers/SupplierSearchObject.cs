using Regira.Entities.Models;

namespace Fleet.Api.Entities.Suppliers;

public record SupplierSearchObject : SearchObject
{
    public string? Title { get; set; }
    public bool? IsActive { get; set; }
    public ICollection<int>? InterventionTypeId { get; set; }
}
