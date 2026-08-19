using System.ComponentModel.DataAnnotations;

namespace RoomPlanner.Api.Entities.Reservations;

public class ReservationAttendeeInputDto
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public int? EmployeeId { get; set; }

    [MaxLength(128)]
    public string? ExternalName { get; set; }

    [MaxLength(256)]
    public string? ExternalEmail { get; set; }

    public AttendeeResponseStatus ResponseStatus { get; set; } = AttendeeResponseStatus.Invited;
}
