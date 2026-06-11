using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Fleet.API.Entities.InterventionTypes;

/// <summary>
/// An editable catalogue of maintenance operations a vehicle can undergo and a supplier can perform
/// (e.g. oil change, brake service, periodic inspection).
/// </summary>
public class InterventionType : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasCode, IHasNormalizedContent
{
    public int Id { get; set; }

    [Required, MaxLength(32)]
    public string? Code { get; set; }

    [Required, MaxLength(128)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    /// <summary>Recommended mileage (km) between two occurrences of this maintenance, when applicable.</summary>
    public int? DefaultIntervalKm { get; set; }

    /// <summary>Indicative duration of the operation in minutes.</summary>
    public int? EstimatedDurationMinutes { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = new[] { nameof(Code), nameof(Title), nameof(Description) })]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
