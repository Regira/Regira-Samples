using System.ComponentModel.DataAnnotations;
using Fleet.Api.Entities.InterventionTypes;
using Fleet.Api.Entities.Invoices;
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
    public int? InvoiceId { get; set; }
    public InvoiceDto? Invoice { get; set; }
    public InterventionStatus Status { get; set; }
    public InterventionPriority Priority { get; set; }
    public DateOnly ScheduledDate { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public int? Mileage { get; set; }
    public string? Description { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<InterventionLineDto>? Lines { get; set; }
}

public class InterventionLineDto
{
    public int Id { get; set; }
    public int InterventionId { get; set; }
    public int InterventionTypeId { get; set; }
    public InterventionTypeDto? InterventionType { get; set; }
    public decimal Cost { get; set; }
    public string? Remarks { get; set; }
}

public class InterventionInputDto
{
    public int Id { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Vehicle is required.")] public int VehicleId { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Supplier is required.")] public int SupplierId { get; set; }
    public int? InvoiceId { get; set; }
    public InterventionStatus Status { get; set; }
    public InterventionPriority Priority { get; set; } = InterventionPriority.Normal;
    public DateOnly ScheduledDate { get; set; }
    public DateOnly? CompletedDate { get; set; }
    [Range(0, 5_000_000)] public int? Mileage { get; set; }
    [MaxLength(512)] public string? Description { get; set; }
    // nullable + uninitialized: omitted = lines untouched (e.g. a status-only PATCH)
    public ICollection<InterventionLineInputDto>? Lines { get; set; }
}

public class InterventionLineInputDto
{
    public int Id { get; set; }
    public int InterventionId { get; set; }
    [Range(1, int.MaxValue)] public int InterventionTypeId { get; set; }
    [Range(0, 1_000_000)] public decimal Cost { get; set; }
    [MaxLength(256)] public string? Remarks { get; set; }
}
