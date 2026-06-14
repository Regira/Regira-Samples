using System.ComponentModel.DataAnnotations;

namespace Fleet.API.Entities.Interventions;

public class InterventionInputDto
{
    public int Id { get; set; }

    public int VehicleId { get; set; }
    public int SupplierId { get; set; }
    public int InterventionTypeId { get; set; }
    public int? InvoiceId { get; set; }

    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public InterventionStatus Status { get; set; }
    public decimal Cost { get; set; }
    public int? MileageAtService { get; set; }

    [MaxLength(1024)]
    public string? Description { get; set; }
}
