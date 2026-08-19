using System.ComponentModel.DataAnnotations;

namespace RoomPlanner.Api.Entities.Floors;

public class FloorInputDto
{
    public int Id { get; set; }
    public int BuildingId { get; set; }

    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;

    public int Level { get; set; }
}
