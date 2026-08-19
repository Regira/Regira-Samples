using RoomPlanner.Api.Entities.Buildings;

namespace RoomPlanner.Api.Entities.Floors;

public class FloorDto
{
    public int Id { get; set; }
    public int BuildingId { get; set; }
    public BuildingDto? Building { get; set; }
    public string Title { get; set; } = null!;
    public int Level { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
