using RoomPlanner.Api.Entities.MeetingRooms;

namespace RoomPlanner.Api.Entities.Reservations;

public class ReservationRoomDto
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public int RoomId { get; set; }
    public MeetingRoomDto? Room { get; set; }
}
