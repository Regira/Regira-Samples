using Regira.Entities.Models;

namespace Fleet.Api.Entities.Interventions;

public record InterventionSearchObject : SearchObject
{
    public ICollection<int>? VehicleId { get; set; }
    public ICollection<int>? SupplierId { get; set; }
    public ICollection<int>? InvoiceId { get; set; }
    public ICollection<int>? InterventionTypeId { get; set; }
    public ICollection<InterventionStatus>? Status { get; set; }
    public ICollection<InterventionPriority>? Priority { get; set; }
    /// <summary>true = billed on an invoice, false = not (yet) invoiced.</summary>
    public bool? IsInvoiced { get; set; }
    public DateOnly? MinDate { get; set; }
    public DateOnly? MaxDate { get; set; }
}

public enum InterventionSortBy
{
    Default = 0,
    ScheduledDate, ScheduledDateDesc,
    TotalCost, TotalCostDesc,
    Status, Priority, PriorityDesc,
    Vehicle, Supplier
}

[Flags]
public enum InterventionIncludes
{
    Default = 0,
    Lines = 1 << 0,
    All = Lines
}
