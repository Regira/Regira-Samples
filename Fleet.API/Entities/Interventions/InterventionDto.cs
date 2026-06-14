using Fleet.API.Entities.InterventionTypes;
using Fleet.API.Entities.Invoices;
using Fleet.API.Entities.Suppliers;
using Fleet.API.Entities.Vehicles;

namespace Fleet.API.Entities.Interventions;

/// <summary>Lightweight intervention projection for nested references (e.g. on an invoice).</summary>
public class InterventionCoreDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int InterventionTypeId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public InterventionStatus Status { get; set; }
    public decimal Cost { get; set; }
    public string? Description { get; set; }
}

public class InterventionDto : InterventionCoreDto
{
    public int SupplierId { get; set; }
    public int? InvoiceId { get; set; }
    public int? MileageAtService { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public VehicleCoreDto? Vehicle { get; set; }
    public SupplierCoreDto? Supplier { get; set; }
    public InterventionTypeDto? InterventionType { get; set; }
    public InvoiceCoreDto? Invoice { get; set; }
}
