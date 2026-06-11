using System.ComponentModel.DataAnnotations;
using Fleet.API.Entities.Common;
using Fleet.API.Entities.InterventionTypes;
using Fleet.API.Entities.Invoices;
using Fleet.API.Entities.Suppliers;
using Fleet.API.Entities.Vehicles;

namespace Fleet.API.Entities.Interventions;

public class InterventionDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public int VehicleId { get; set; }
    public VehicleDto? Vehicle { get; set; }
    public int InterventionTypeId { get; set; }
    public InterventionTypeDto? InterventionType { get; set; }
    public int SupplierId { get; set; }
    public SupplierDto? Supplier { get; set; }
    public int? InvoiceId { get; set; }
    public InvoiceDto? Invoice { get; set; }
    public InterventionStatus Status { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int MileageAtService { get; set; }
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class InterventionInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(32)] public string? Code { get; set; }
    public int VehicleId { get; set; }
    public int InterventionTypeId { get; set; }
    public int SupplierId { get; set; }
    public int? InvoiceId { get; set; }
    public InterventionStatus Status { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int MileageAtService { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    public decimal Cost { get; set; }
}
