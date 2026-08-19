using Regira.Entities.Models.Abstractions;
using RoomPlanner.Api.Entities.MeetingRooms;

namespace RoomPlanner.Api.Entities.Reservations;

/// <summary>Many-to-many join between Reservation and MeetingRoom - owned by Reservation via e.Related().</summary>
public class ReservationRoom : IEntityWithSerial
{
    public int Id { get; set; }

    public int ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    public int RoomId { get; set; }
    public MeetingRoom? Room { get; set; }
}
