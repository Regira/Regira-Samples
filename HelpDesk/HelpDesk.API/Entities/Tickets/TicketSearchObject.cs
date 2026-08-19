using Regira.Entities.Models;

namespace HelpDesk.API.Entities.Tickets;

public record TicketSearchObject : SearchObject
{
    public ICollection<int>? CategoryId { get; set; }
    public ICollection<int>? PriorityId { get; set; }
    public ICollection<int>? StatusId { get; set; }
    public ICollection<int>? SupportTeamId { get; set; }
    public ICollection<int>? CustomerId { get; set; }
    public ICollection<int>? AssignedEmployeeId { get; set; }
    public bool? IsUnassigned { get; set; }
    public bool? IsClosed { get; set; }
}
