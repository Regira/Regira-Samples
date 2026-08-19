using RoomPlanner.Api.Entities.Floors;

namespace RoomPlanner.Api.Entities.MeetingRooms;

public class MeetingRoomDto
{
    public int Id { get; set; }
    public int FloorId { get; set; }
    public FloorDto? Floor { get; set; }
    public string Title { get; set; } = null!;
    public int Capacity { get; set; }
    public RoomEquipment Equipment { get; set; }
    public bool RequiresApproval { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
