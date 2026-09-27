using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using RoomPlanner.Api.Entities.Employees;
using RoomPlanner.Api.Entities.Rooms;

namespace RoomPlanner.Api.Entities.Reservations;

public enum ReservationStatus
{
    /// <summary>At least one room still awaits approval.</summary>
    Pending = 0,
    /// <summary>Every room is approved (automatically or by a facility manager).</summary>
    Approved,
    /// <summary>Some rooms approved, others rejected.</summary>
    PartiallyApproved,
    /// <summary>Every room was rejected.</summary>
    Rejected,
    /// <summary>Cancelled by the organizer - rooms are released.</summary>
    Cancelled,
}

public enum RoomApprovalStatus
{
    Pending = 0,
    Approved,
    Rejected,
}

public enum AttendeeResponse
{
    NoResponse = 0,
    Accepted,
    Tentative,
    Declined,
}

/// <summary>A meeting booked by an organizer for one or more rooms, with invited attendees.</summary>
public class Reservation : IEntityWithSerial, IHasTitle, IHasDescription, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(2048)] public string? Description { get; set; }
    public int OrganizerId { get; set; }
    public Employee? Organizer { get; set; }
    /// <summary>UTC start instant.</summary>
    public DateTime Start { get; set; }
    /// <summary>UTC end instant (exclusive).</summary>
    public DateTime End { get; set; }

    // --- server-owned (derived / workflow) - written by ReservationPrepper and the workflow actions only ---
    public ReservationStatus Status { get; set; }
    public int AttendeeCount { get; set; }
    public DateTime? CancelledOn { get; set; }
    [MaxLength(512)] public string? CancelReason { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    /// <summary>Owned rows: the booked rooms, each with its own approval state.</summary>
    public ICollection<ReservationRoom>? Rooms { get; set; }
    /// <summary>Owned rows: invited employees.</summary>
    public ICollection<ReservationAttendee>? Attendees { get; set; }
}

/// <summary>Owned join row Reservation - Room. Carries the per-room approval.</summary>
public class ReservationRoom : IEntityWithSerial
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public Reservation? Reservation { get; set; }
    public int RoomId { get; set; }
    public Room? Room { get; set; }
    public RoomApprovalStatus ApprovalStatus { get; set; }
    public DateTime? DecidedOn { get; set; }
    [MaxLength(512)] public string? DecisionNote { get; set; }
}

/// <summary>Owned join row Reservation - Employee (invitee).</summary>
public class ReservationAttendee : IEntityWithSerial
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public Reservation? Reservation { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public AttendeeResponse Response { get; set; }
    public bool IsOptional { get; set; }
}
