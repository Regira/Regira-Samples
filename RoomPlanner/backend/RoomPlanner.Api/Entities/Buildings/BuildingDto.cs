namespace RoomPlanner.Api.Entities.Buildings;

public class BuildingDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
