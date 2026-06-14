using Regira.Entities.Models;

namespace Fleet.API.Entities.Interventions;

public record InterventionSearchObject : SearchObject
{
    public ICollection<int>? VehicleId { get; set; }
    public ICollection<int>? SupplierId { get; set; }
    public ICollection<int>? InterventionTypeId { get; set; }
    public ICollection<int>? InvoiceId { get; set; }
    /// <summary>Filter on whether the intervention has been billed to an invoice.</summary>
    public bool? HasInvoice { get; set; }
    public ICollection<InterventionStatus>? Status { get; set; }
    public DateTime? MinScheduledDate { get; set; }
    public DateTime? MaxScheduledDate { get; set; }
    public decimal? MinCost { get; set; }
    public decimal? MaxCost { get; set; }
}
