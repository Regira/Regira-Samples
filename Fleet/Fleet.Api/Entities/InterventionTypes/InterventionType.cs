using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Fleet.Api.Entities.InterventionTypes;

public enum InterventionCategory { Maintenance = 0, Repair, Inspection, Tires, Bodywork, Electrical, Cleaning }

public class InterventionType : IEntityWithSerial, IHasTitle, IHasCode, IHasDescription, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string? Title { get; set; }
    [Required, MaxLength(16)] public string? Code { get; set; }
    [MaxLength(512)] public string? Description { get; set; }
    public InterventionCategory Category { get; set; }
    /// <summary>Recommended interval in km (null = not mileage based).</summary>
    public int? IntervalKm { get; set; }
    /// <summary>Recommended interval in months (null = not time based).</summary>
    public int? IntervalMonths { get; set; }
    /// <summary>Indicative cost, used as default when planning an intervention.</summary>
    public decimal StandardCost { get; set; }
    public bool IsActive { get; set; } = true;

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Code)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class InterventionTypeDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public InterventionCategory Category { get; set; }
    public int? IntervalKm { get; set; }
    public int? IntervalMonths { get; set; }
    public decimal StandardCost { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class InterventionTypeInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string? Title { get; set; }
    [Required, MaxLength(16)] public string? Code { get; set; }
    [MaxLength(512)] public string? Description { get; set; }
    public InterventionCategory Category { get; set; }
    [Range(0, 1_000_000)] public int? IntervalKm { get; set; }
    [Range(0, 240)] public int? IntervalMonths { get; set; }
    [Range(0, 1_000_000)] public decimal StandardCost { get; set; }
    public bool IsActive { get; set; } = true;
}

public record InterventionTypeSearchObject : SearchObject
{
    public ICollection<InterventionCategory>? Category { get; set; }
    public bool? IsActive { get; set; }
}
