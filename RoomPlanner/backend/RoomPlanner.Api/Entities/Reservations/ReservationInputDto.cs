using System.ComponentModel.DataAnnotations;

namespace RoomPlanner.Api.Entities.Reservations;

public class ReservationInputDto
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = null!;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int OrganizerId { get; set; }

    /// <summary>
    /// Set on approve/reject workflows via PATCH; on create the server computes the initial value
    /// from the selected rooms' RequiresApproval flag (see ReservationManager.Add).
    /// </summary>
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    // Nullable + uninitialized: null = untouched by e.Related() sync, [] = delete-all.
    public ICollection<ReservationRoomInputDto>? Rooms { get; set; }
    public ICollection<ReservationAttendeeInputDto>? Attendees { get; set; }
}
