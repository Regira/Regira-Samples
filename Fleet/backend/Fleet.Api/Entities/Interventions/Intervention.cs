using System.ComponentModel.DataAnnotations;
using Fleet.Api.Entities.InterventionTypes;
using Fleet.Api.Entities.Invoices;
using Fleet.Api.Entities.Suppliers;
using Fleet.Api.Entities.Vehicles;
using Regira.Entities.Models.Abstractions;

namespace Fleet.Api.Entities.Interventions;

public class Intervention : IEntityWithSerial, IHasTimestamps
{
    public int Id { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public InterventionStatus Status { get; set; } = InterventionStatus.Scheduled;
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    [MaxLength(2048)] public string? Notes { get; set; }
    public decimal Cost { get; set; }

    // Optional parent FK — Invoice.TotalAmount is aggregated from these, NOT via e.Related().
    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public ICollection<InterventionInterventionType>? InterventionTypes { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

// Owned m2m join entity — no own .For<>() registration.
public class InterventionInterventionType : IEntityWithSerial
{
    public int Id { get; set; }
    public int InterventionId { get; set; }
    public Intervention? Intervention { get; set; }
    public int InterventionTypeId { get; set; }
    public InterventionType? InterventionType { get; set; }
}
