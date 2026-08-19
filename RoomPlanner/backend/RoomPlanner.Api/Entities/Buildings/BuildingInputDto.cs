using System.ComponentModel.DataAnnotations;

namespace RoomPlanner.Api.Entities.Buildings;

public class BuildingInputDto
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Title { get; set; } = null!;

    [MaxLength(512)]
    public string? Description { get; set; }

    [Required, MaxLength(256)]
    public string Address { get; set; } = null!;

    [Required, MaxLength(64)]
    public string City { get; set; } = null!;
}
