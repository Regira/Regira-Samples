using EventPlanner.Api.Entities.EventCategories;
using EventPlanner.Api.Entities.Locations;
using EventPlanner.Api.Entities.Sessions;

namespace EventPlanner.Api.Entities.Events;

public class EventDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? BannerImageUrl { get; set; }
    public int LocationId { get; set; }
    public LocationDto? Location { get; set; }
    public int EventCategoryId { get; set; }
    public EventCategoryDto? EventCategory { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsFeatured { get; set; }
    public int? SessionCount { get; set; }
    public int? RegistrationCount { get; set; }
    public ICollection<SessionCoreDto>? Sessions { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

// Lightweight nested DTO — used on Event.Sessions to avoid an Event <-> Session cycle.
public class SessionCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Room { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; }
    public int? SeatsTaken { get; set; }
}

// Lightweight nested DTO — used on Session.Event / Registration.Event to avoid a full-graph cycle.
public class EventCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? BannerImageUrl { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
