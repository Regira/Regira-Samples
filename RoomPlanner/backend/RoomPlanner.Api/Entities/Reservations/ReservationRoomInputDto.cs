namespace RoomPlanner.Api.Entities.Reservations;

public class ReservationRoomInputDto
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public int RoomId { get; set; }
}
