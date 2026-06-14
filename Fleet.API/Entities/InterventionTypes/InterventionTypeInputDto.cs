using System.ComponentModel.DataAnnotations;

namespace Fleet.API.Entities.InterventionTypes;

public class InterventionTypeInputDto
{
    public int Id { get; set; }

    [MaxLength(32)]
    public string? Code { get; set; }

    [Required, MaxLength(128)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    public int? EstimatedDurationMinutes { get; set; }
}
