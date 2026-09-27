using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EventPlanner.Api.Entities.Categories;
using EventPlanner.Api.Entities.Locations;
using EventPlanner.Api.Entities.Speakers;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace EventPlanner.Api.Entities.Events;

public enum EventStatus { Draft = 0, Published = 1, Cancelled = 2 }

/// <summary>An event at one location, spanning one or more (calendar) days. Aggregate root of its sessions.</summary>
public class Event : IEntityWithSerial, IHasTitle, IHasDescription, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(160)] public string? Title { get; set; }
    [MaxLength(320)] public string? Summary { get; set; }
    [MaxLength(8000)] public string? Description { get; set; }

    public int CategoryId { get; set; }
    public EventCategory? Category { get; set; }
    public int LocationId { get; set; }
    public Location? Location { get; set; }

    /// <summary>First day (calendar date, no time zone).</summary>
    public DateOnly StartDate { get; set; }
    /// <summary>Last day (inclusive).</summary>
    public DateOnly EndDate { get; set; }

    public EventStatus Status { get; set; } = EventStatus.Draft;
    public bool IsFeatured { get; set; }
    /// <summary>Maximum number of (confirmed) participants; null = unlimited. Extra registrations go on the wait list.</summary>
    public int? MaxParticipants { get; set; }
    [MaxLength(512)] public string? BannerUrl { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Summary)])]
    public string? NormalizedContent { get; set; }

    public ICollection<Session>? Sessions { get; set; }

    // filled by EventProcessor
    [NotMapped] public int? RegistrationCount { get; set; }
    [NotMapped] public int? WaitlistCount { get; set; }
    /// <summary>The caller's own registration for this event (any status, cancelled included).</summary>
    [NotMapped] public int? MyRegistrationId { get; set; }
}

/// <summary>Owned child of <see cref="Event"/> — synced through the event's Related() configuration.</summary>
public class Session : IEntityWithSerial
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
    [Required, MaxLength(160)] public string? Title { get; set; }
    [MaxLength(4000)] public string? Description { get; set; }
    /// <summary>UTC instant.</summary>
    public DateTime StartTime { get; set; }
    /// <summary>UTC instant.</summary>
    public DateTime EndTime { get; set; }
    [MaxLength(96)] public string? Room { get; set; }
    [MaxLength(64)] public string? Track { get; set; }
    public int Capacity { get; set; }

    public ICollection<SessionSpeaker>? Speakers { get; set; }

    // filled by EventProcessor
    [NotMapped] public int? RegisteredCount { get; set; }
}

/// <summary>Owned join row Session x Speaker (nested Related()).</summary>
public class SessionSpeaker : IEntityWithSerial
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public Session? Session { get; set; }
    public int SpeakerId { get; set; }
    public Speaker? Speaker { get; set; }
}
