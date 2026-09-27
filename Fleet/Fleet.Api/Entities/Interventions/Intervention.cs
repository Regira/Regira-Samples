using System.ComponentModel.DataAnnotations;
using Fleet.Api.Entities.InterventionTypes;
using Fleet.Api.Entities.Invoices;
using Fleet.Api.Entities.Suppliers;
using Fleet.Api.Entities.Vehicles;
using Regira.Entities.Models.Abstractions;

namespace Fleet.Api.Entities.Interventions;

public enum InterventionStatus { Planned = 0, InProgress, Completed, Cancelled }
public enum InterventionPriority { Low = 0, Normal, High, Urgent }

public class Intervention : IEntityWithSerial, IHasDescription, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    /// <summary>Optional: the supplier invoice this intervention is billed on. Written by the intervention (one writer).</summary>
    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public InterventionStatus Status { get; set; }
    public InterventionPriority Priority { get; set; } = InterventionPriority.Normal;
    public DateOnly ScheduledDate { get; set; }
    public DateOnly? CompletedDate { get; set; }
    /// <summary>Odometer reading at the time of the intervention.</summary>
    public int? Mileage { get; set; }
    [MaxLength(512)] public string? Description { get; set; }

    /// <summary>Sum of the line costs - recomputed by <see cref="InterventionPrepper"/> on every save, never client input.</summary>
    public decimal TotalCost { get; set; }

    /// <summary>Folded by <see cref="InterventionNormalizer"/>: vehicle plate, supplier name, type titles, description.</summary>
    [MaxLength(1024)] public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    /// <summary>The intervention types performed (owned rows, synced via Related()).</summary>
    public ICollection<InterventionLine>? Lines { get; set; }
}

/// <summary>Owned child: one intervention type performed within an intervention, with its cost.</summary>
public class InterventionLine : IEntityWithSerial
{
    public int Id { get; set; }
    public int InterventionId { get; set; }
    public Intervention? Intervention { get; set; }
    public int InterventionTypeId { get; set; }
    public InterventionType? InterventionType { get; set; }
    public decimal Cost { get; set; }
    [MaxLength(256)] public string? Remarks { get; set; }
}
