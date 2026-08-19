using EventPlanner.Api.Entities.Sessions;
using Regira.Entities.Models.Abstractions;

namespace EventPlanner.Api.Entities.Registrations;

// Owned many-to-many join entity — managed via e.Related() on Registration, no own .For<>() registration
public class RegistrationSession : IEntityWithSerial
{
    public int Id { get; set; }
    public int RegistrationId { get; set; }
    public Registration? Registration { get; set; }
    public int SessionId { get; set; }
    public Session? Session { get; set; }
}
