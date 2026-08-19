using RoomPlanner.Api.Entities.Employees;

namespace RoomPlanner.Api.Entities.Reservations;

public class ReservationAttendeeDto
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public int? EmployeeId { get; set; }
    public EmployeeDto? Employee { get; set; }
    public string? ExternalName { get; set; }
    public string? ExternalEmail { get; set; }
    public AttendeeResponseStatus ResponseStatus { get; set; }
}
