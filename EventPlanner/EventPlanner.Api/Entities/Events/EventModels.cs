using System.ComponentModel.DataAnnotations;
using EventPlanner.Api.Entities.Categories;
using EventPlanner.Api.Entities.Locations;
using EventPlanner.Api.Entities.Speakers;
using Regira.Entities.Models;

namespace EventPlanner.Api.Entities.Events;

public record EventSearchObject : SearchObject
{
    public ICollection<int>? CategoryId { get; set; }
    public ICollection<int>? LocationId { get; set; }
    public ICollection<int>? SpeakerId { get; set; }
    public ICollection<EventStatus>? Status { get; set; }
    public bool? IsFeatured { get; set; }
    /// <summary>Events still running on or after this day.</summary>
    public DateOnly? MinDate { get; set; }
    /// <summary>Events starting on or before this day.</summary>
    public DateOnly? MaxDate { get; set; }
    /// <summary>true = not yet ended; false = ended.</summary>
    public bool? Upcoming { get; set; }
}

public enum EventSortBy { StartDate = 0, StartDateDesc, Title, Created }

[Flags] public enum EventIncludes { Default = 0, Sessions = 1 << 0, All = Sessions }

public class EventDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public EventCategoryDto? Category { get; set; }
    public int LocationId { get; set; }
    public LocationDto? Location { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public EventStatus Status { get; set; }
    public bool IsFeatured { get; set; }
    public int? MaxParticipants { get; set; }
    public string? BannerUrl { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<SessionDto>? Sessions { get; set; }
    public int? RegistrationCount { get; set; }
    public int? WaitlistCount { get; set; }
    public int? MyRegistrationId { get; set; }
}

public class SessionDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Room { get; set; }
    public string? Track { get; set; }
    public int Capacity { get; set; }
    public int? RegisteredCount { get; set; }
    public ICollection<SessionSpeakerDto>? Speakers { get; set; }
}

public class SessionSpeakerDto
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int SpeakerId { get; set; }
    public SpeakerDto? Speaker { get; set; }
}

public class EventInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(160)] public string? Title { get; set; }
    [MaxLength(320)] public string? Summary { get; set; }
    [MaxLength(8000)] public string? Description { get; set; }
    [Range(1, int.MaxValue)] public int CategoryId { get; set; }
    [Range(1, int.MaxValue)] public int LocationId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public EventStatus Status { get; set; }
    public bool IsFeatured { get; set; }
    [Range(1, 100000)] public int? MaxParticipants { get; set; }
    [MaxLength(512)] public string? BannerUrl { get; set; }
    // nullable + uninitialized: omitted = untouched, [] = delete all
    public ICollection<SessionInputDto>? Sessions { get; set; }
}

public class SessionInputDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    [Required, MaxLength(160)] public string? Title { get; set; }
    [MaxLength(4000)] public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    [MaxLength(96)] public string? Room { get; set; }
    [MaxLength(64)] public string? Track { get; set; }
    [Range(1, 100000)] public int Capacity { get; set; }
    public ICollection<SessionSpeakerInputDto>? Speakers { get; set; }
}

public class SessionSpeakerInputDto
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int SpeakerId { get; set; }
}
