using Regira.Entities.Models;

namespace EventPlanner.Api.Entities.Sessions;

public record SessionSearchObject : SearchObject
{
    public ICollection<int>? EventId { get; set; }
    public ICollection<int>? SpeakerId { get; set; }
    public DateTime? MinStartTime { get; set; }
    public DateTime? MaxStartTime { get; set; }
}
