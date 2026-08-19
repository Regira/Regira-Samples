using RoomPlanner.Api.Entities.Employees;

namespace RoomPlanner.Api.Entities.Reservations;

public class ReservationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int OrganizerId { get; set; }
    public EmployeeDto? Organizer { get; set; }
    public ReservationStatus Status { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<ReservationRoomDto>? Rooms { get; set; }
    public ICollection<ReservationAttendeeDto>? Attendees { get; set; }
}
