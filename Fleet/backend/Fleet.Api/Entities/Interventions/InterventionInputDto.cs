using System.ComponentModel.DataAnnotations;

namespace Fleet.Api.Entities.Interventions;

public class InterventionInputDto
{
    public int Id { get; set; }
    [Required] public int VehicleId { get; set; }
    [Required] public int SupplierId { get; set; }
    public InterventionStatus Status { get; set; } = InterventionStatus.Scheduled;
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    [MaxLength(2048)] public string? Notes { get; set; }
    public decimal Cost { get; set; }

    // Managed via Intervention's own service — Invoice does NOT own this write (Related() decision table).
    public int? InvoiceId { get; set; }

    // Only present because configured with e.Related(...) — nullable + uninitialized:
    // null = untouched, [] = delete-all, populated = the new set.
    public ICollection<InterventionInterventionTypeInputDto>? InterventionTypes { get; set; }
}

public class InterventionInterventionTypeInputDto
{
    public int Id { get; set; }
    public int InterventionId { get; set; }
    public int InterventionTypeId { get; set; }
}
