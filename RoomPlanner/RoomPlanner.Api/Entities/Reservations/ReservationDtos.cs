using System.ComponentModel.DataAnnotations;
using RoomPlanner.Api.Entities.Employees;
using RoomPlanner.Api.Entities.Rooms;

namespace RoomPlanner.Api.Entities.Reservations;

public class ReservationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int OrganizerId { get; set; }
    public EmployeeDto? Organizer { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public ReservationStatus Status { get; set; }
    public int AttendeeCount { get; set; }
    public DateTime? CancelledOn { get; set; }
    public string? CancelReason { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<ReservationRoomDto>? Rooms { get; set; }
    public ICollection<ReservationAttendeeDto>? Attendees { get; set; }
}

public class ReservationRoomDto
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public int RoomId { get; set; }
    public RoomDto? Room { get; set; }
    public RoomApprovalStatus ApprovalStatus { get; set; }
    public DateTime? DecidedOn { get; set; }
    public string? DecisionNote { get; set; }
}

public class ReservationAttendeeDto
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public int EmployeeId { get; set; }
    public EmployeeDto? Employee { get; set; }
    public AttendeeResponse Response { get; set; }
    public bool IsOptional { get; set; }
}

/// <summary>
/// Status, AttendeeCount, CancelledOn/CancelReason and the per-room approval fields are NOT here:
/// they are derived or written by the workflow actions and restored by <see cref="ReservationPrepper"/>.
/// </summary>
public class ReservationInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(2048)] public string? Description { get; set; }
    public int OrganizerId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public ICollection<ReservationRoomInputDto>? Rooms { get; set; }
    public ICollection<ReservationAttendeeInputDto>? Attendees { get; set; }
}

public class ReservationRoomInputDto
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public int RoomId { get; set; }
}

public class ReservationAttendeeInputDto
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public int EmployeeId { get; set; }
    public AttendeeResponse Response { get; set; }
    public bool IsOptional { get; set; }
}

/// <summary>Body of the workflow actions (approve / reject / cancel).</summary>
public class DecisionInput
{
    [MaxLength(512)] public string? Note { get; set; }
}
