using Fleet.API.Entities.Common;
using Regira.Entities.Models;

namespace Fleet.API.Entities.Interventions;

public record InterventionSearchObject : SearchObject
{
    public ICollection<int>? VehicleId { get; set; }
    public ICollection<int>? SupplierId { get; set; }
    public ICollection<int>? InterventionTypeId { get; set; }
    public ICollection<int>? InvoiceId { get; set; }
    public ICollection<InterventionStatus>? Status { get; set; }

    /// <summary>true = only invoiced interventions, false = only not-yet-invoiced.</summary>
    public bool? IsInvoiced { get; set; }

    public DateTime? MinScheduledDate { get; set; }
    public DateTime? MaxScheduledDate { get; set; }
    public decimal? MinCost { get; set; }
    public decimal? MaxCost { get; set; }
}
