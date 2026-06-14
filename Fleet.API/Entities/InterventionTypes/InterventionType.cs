using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Fleet.API.Entities.InterventionTypes;

/// <summary>
/// An editable catalog of maintenance operations a vehicle can undergo
/// (e.g. oil change, brake inspection, tyre replacement).
/// </summary>
public class InterventionType : IEntityWithSerial, IHasCode, IHasTitle, IHasDescription, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }

    [MaxLength(32)]
    public string? Code { get; set; }

    [Required, MaxLength(128)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    /// <summary>Indicative duration of the operation, in minutes.</summary>
    public int? EstimatedDurationMinutes { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Code), nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
