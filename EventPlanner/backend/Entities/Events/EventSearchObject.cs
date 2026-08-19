using Regira.Entities.Models;

namespace EventPlanner.Api.Entities.Events;

public record EventSearchObject : SearchObject
{
    public ICollection<int>? LocationId { get; set; }
    public ICollection<int>? EventCategoryId { get; set; }
    public DateTime? MinStartDate { get; set; }
    public DateTime? MaxStartDate { get; set; }
    public bool? IsFeatured { get; set; }
}
