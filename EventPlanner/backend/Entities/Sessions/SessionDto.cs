using EventPlanner.Api.Entities.Events;
using EventPlanner.Api.Entities.Speakers;

namespace EventPlanner.Api.Entities.Sessions;

public class SessionDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public EventCoreDto? Event { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Room { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; }
    public int? SeatsTaken { get; set; }
    public ICollection<SessionSpeakerDto>? SessionSpeakers { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class SessionSpeakerDto
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int SpeakerId { get; set; }
    public SpeakerDto? Speaker { get; set; }
}
