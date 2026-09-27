using Regira.Entities.Models;

namespace RoomPlanner.Api.Entities.Reservations;

public record ReservationSearchObject : SearchObject
{
    /// <summary>With To: reservations overlapping the [From, To) window (either bound may be used alone).</summary>
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public ICollection<int>? RoomId { get; set; }
    public ICollection<int>? BuildingId { get; set; }
    public ICollection<int>? OrganizerId { get; set; }
    /// <summary>Reservations where the employee is organizer OR attendee.</summary>
    public ICollection<int>? EmployeeId { get; set; }
    public ICollection<ReservationStatus>? Status { get; set; }
    /// <summary>true: at least one room row awaits approval.</summary>
    public bool? AwaitingApproval { get; set; }
}

public enum ReservationSortBy
{
    Default = 0,
    Start,
    StartDesc,
    Title,
    CreatedDesc,
}

[Flags]
public enum ReservationIncludes
{
    Default = 0,
    Rooms = 1 << 0,
    Attendees = 1 << 1,
    All = Rooms | Attendees,
}
