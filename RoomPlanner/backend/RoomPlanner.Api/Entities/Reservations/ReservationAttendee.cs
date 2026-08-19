using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using RoomPlanner.Api.Entities.Employees;

namespace RoomPlanner.Api.Entities.Reservations;

/// <summary>Owned child of Reservation via e.Related() - an invited attendee, internal (Employee) or external.</summary>
public class ReservationAttendee : IEntityWithSerial
{
    public int Id { get; set; }

    public int ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    [MaxLength(128)]
    public string? ExternalName { get; set; }

    [MaxLength(256)]
    public string? ExternalEmail { get; set; }

    public AttendeeResponseStatus ResponseStatus { get; set; } = AttendeeResponseStatus.Invited;
}
