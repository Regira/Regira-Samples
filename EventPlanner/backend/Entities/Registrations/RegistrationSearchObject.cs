using Regira.Entities.Models;

namespace EventPlanner.Api.Entities.Registrations;

public record RegistrationSearchObject : SearchObject
{
    public ICollection<int>? EmployeeId { get; set; }
    public ICollection<int>? EventId { get; set; }
    public ICollection<int>? SessionId { get; set; }
    public ICollection<RegistrationStatus>? Status { get; set; }
}
