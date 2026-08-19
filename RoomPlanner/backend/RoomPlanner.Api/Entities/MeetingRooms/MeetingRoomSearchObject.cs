using Regira.Entities.Models;

namespace RoomPlanner.Api.Entities.MeetingRooms;

public record MeetingRoomSearchObject : SearchObject
{
    public ICollection<int>? FloorId { get; set; }
    public ICollection<int>? BuildingId { get; set; }
    public int? MinCapacity { get; set; }

    /// <summary>Room must have ALL requested flags set.</summary>
    public RoomEquipment? Equipment { get; set; }
    public bool? IsActive { get; set; }
    public bool? RequiresApproval { get; set; }
}
