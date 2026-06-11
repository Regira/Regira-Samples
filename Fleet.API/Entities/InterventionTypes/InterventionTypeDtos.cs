using System.ComponentModel.DataAnnotations;

namespace Fleet.API.Entities.InterventionTypes;

public class InterventionTypeDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int? DefaultIntervalKm { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class InterventionTypeInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(32)] public string? Code { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    public int? DefaultIntervalKm { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
}
