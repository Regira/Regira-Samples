using System.ComponentModel.DataAnnotations;

namespace EventPlanner.Api.Entities.Registrations;

public class RegistrationInputDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int EventId { get; set; }
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;
    [MaxLength(1024)] public string? Notes { get; set; }

    // Declared here because it is configured with e.Related() below — nullable + uninitialized,
    // so an omitted collection maps as null (untouched) instead of [] (delete-all).
    public ICollection<RegistrationSessionInputDto>? SelectedSessions { get; set; }
}

public class RegistrationSessionInputDto
{
    public int Id { get; set; }
    public int RegistrationId { get; set; }
    public int SessionId { get; set; }
}
