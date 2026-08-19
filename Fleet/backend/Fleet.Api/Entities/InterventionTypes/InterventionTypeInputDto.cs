using System.ComponentModel.DataAnnotations;

namespace Fleet.Api.Entities.InterventionTypes;

public class InterventionTypeInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    public decimal EstimatedCost { get; set; }
    public double EstimatedDurationHours { get; set; }
}
