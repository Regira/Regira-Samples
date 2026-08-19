using System.ComponentModel.DataAnnotations;
using EventPlanner.Api.Entities.Employees;
using EventPlanner.Api.Entities.Events;
using Regira.Entities.Models.Abstractions;

namespace EventPlanner.Api.Entities.Registrations;

public class Registration : IEntityWithSerial, IHasTimestamps
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;
    [MaxLength(1024)] public string? Notes { get; set; }

    public ICollection<RegistrationSession>? SelectedSessions { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
