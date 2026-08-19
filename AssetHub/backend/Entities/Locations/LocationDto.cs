namespace AssetHub.Api.Entities.Locations;

public class LocationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Building { get; set; }
    public string? Room { get; set; }
    public string? Address { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
