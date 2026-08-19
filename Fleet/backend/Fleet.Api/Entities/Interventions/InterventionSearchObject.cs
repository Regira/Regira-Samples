using Regira.Entities.Models;

namespace Fleet.Api.Entities.Interventions;

public record InterventionSearchObject : SearchObject
{
    public ICollection<int>? VehicleId { get; set; }
    public ICollection<int>? SupplierId { get; set; }
    public ICollection<int>? InterventionTypeId { get; set; }
    public ICollection<InterventionStatus>? Status { get; set; }
    public ICollection<int>? InvoiceId { get; set; }
    public bool? HasInvoice { get; set; }
    public DateTime? MinScheduledDate { get; set; }
    public DateTime? MaxScheduledDate { get; set; }
}
