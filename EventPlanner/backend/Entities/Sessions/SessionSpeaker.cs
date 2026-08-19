using EventPlanner.Api.Entities.Speakers;
using Regira.Entities.Models.Abstractions;

namespace EventPlanner.Api.Entities.Sessions;

// Owned many-to-many join entity — managed via e.Related() on Session, no own .For<>() registration
public class SessionSpeaker : IEntityWithSerial
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public Session? Session { get; set; }
    public int SpeakerId { get; set; }
    public Speaker? Speaker { get; set; }
}
