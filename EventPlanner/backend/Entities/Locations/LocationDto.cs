namespace EventPlanner.Api.Entities.Locations;

public class LocationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string? PostalCode { get; set; }
    public string Country { get; set; } = null!;
    public int Capacity { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
