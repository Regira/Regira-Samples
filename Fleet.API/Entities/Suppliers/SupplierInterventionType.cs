using Fleet.API.Entities.InterventionTypes;
using Regira.Entities.Models.Abstractions;

namespace Fleet.API.Entities.Suppliers;

/// <summary>Join entity linking a <see cref="Supplier"/> to an <see cref="InterventionType"/> it can perform.</summary>
public class SupplierInterventionType : IEntityWithSerial
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public int InterventionTypeId { get; set; }
    public InterventionType? InterventionType { get; set; }
}
