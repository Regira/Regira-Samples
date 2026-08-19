using System.ComponentModel.DataAnnotations;

namespace RoomPlanner.Api.Entities.MeetingRooms;

public class MeetingRoomInputDto
{
    public int Id { get; set; }
    public int FloorId { get; set; }

    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;

    public int Capacity { get; set; }
    public RoomEquipment Equipment { get; set; } = RoomEquipment.None;
    public bool RequiresApproval { get; set; }
    public bool IsActive { get; set; } = true;
}
