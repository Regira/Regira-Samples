using Regira.Entities.Models;

namespace RoomPlanner.Api.Entities.Floors;

public record FloorSearchObject : SearchObject
{
    public ICollection<int>? BuildingId { get; set; }
}
