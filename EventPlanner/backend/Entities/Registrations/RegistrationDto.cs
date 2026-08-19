using EventPlanner.Api.Entities.Employees;
using EventPlanner.Api.Entities.Events;

namespace EventPlanner.Api.Entities.Registrations;

public class RegistrationDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public EmployeeDto? Employee { get; set; }
    public int EventId { get; set; }
    public EventCoreDto? Event { get; set; }
    public RegistrationStatus Status { get; set; }
    public string? Notes { get; set; }
    public ICollection<RegistrationSessionDto>? SelectedSessions { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class RegistrationSessionDto
{
    public int Id { get; set; }
    public int RegistrationId { get; set; }
    public int SessionId { get; set; }
    public SessionCoreDto? Session { get; set; }
}
