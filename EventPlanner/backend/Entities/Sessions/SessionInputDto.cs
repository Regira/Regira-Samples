using System.ComponentModel.DataAnnotations;

namespace EventPlanner.Api.Entities.Sessions;

public class SessionInputDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(2048)] public string? Description { get; set; }
    [MaxLength(64)] public string? Room { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; }

    // Only declared here because it is configured with e.Related() below — nullable + uninitialized,
    // so an omitted collection maps as null (untouched) instead of [] (delete-all).
    public ICollection<SessionSpeakerInputDto>? SessionSpeakers { get; set; }
}

public class SessionSpeakerInputDto
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int SpeakerId { get; set; }
}
