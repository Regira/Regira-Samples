using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EventPlanner.Api.Entities.Events;
using Regira.Entities.Models.Abstractions;

namespace EventPlanner.Api.Entities.Sessions;

public class Session : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(2048)] public string? Description { get; set; }
    [MaxLength(64)] public string? Room { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; }

    public ICollection<SessionSpeaker>? SessionSpeakers { get; set; }

    // Filled by SessionProcessor from RegistrationSessions (a non-owned, cross-entity aggregate)
    [NotMapped] public int? SeatsTaken { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
