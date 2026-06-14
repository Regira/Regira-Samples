namespace Fleet.API.Entities.InterventionTypes;

public class InterventionTypeDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
