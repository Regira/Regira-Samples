namespace Fleet.Api.Entities.InterventionTypes;

public class InterventionTypeDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal EstimatedCost { get; set; }
    public double EstimatedDurationHours { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
