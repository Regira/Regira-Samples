using Fleet.Api.Entities.InterventionTypes;
using Fleet.Api.Entities.Suppliers;
using Fleet.Api.Entities.Vehicles;

namespace Fleet.Api.Entities.Interventions;

public class InterventionDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public VehicleDto? Vehicle { get; set; }
    public int SupplierId { get; set; }
    public SupplierDto? Supplier { get; set; }
    public InterventionStatus Status { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Notes { get; set; }
    public decimal Cost { get; set; }
    public int? InvoiceId { get; set; }
    public ICollection<InterventionInterventionTypeDto>? InterventionTypes { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class InterventionInterventionTypeDto
{
    public int Id { get; set; }
    public int InterventionId { get; set; }
    public int InterventionTypeId { get; set; }
    public InterventionTypeDto? InterventionType { get; set; }
}
