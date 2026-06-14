using System.ComponentModel.DataAnnotations;
using Fleet.API.Entities.InterventionTypes;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Fleet.API.Entities.Vehicles;

/// <summary>
/// A fleet vehicle (car, van, truck, ...) that undergoes maintenance interventions.
/// </summary>
public class Vehicle : IEntityWithSerial, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }

    [Required, MaxLength(16)]
    public string LicensePlate { get; set; } = null!;

    [Required, MaxLength(64)]
    public string Brand { get; set; } = null!;

    [Required, MaxLength(64)]
    public string Model { get; set; } = null!;

    public VehicleType VehicleType { get; set; }

    [MaxLength(32)]
    public string? Vin { get; set; }

    public int? Year { get; set; }

    /// <summary>Current odometer reading in kilometres.</summary>
    public int Mileage { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(LicensePlate), nameof(Brand), nameof(Model), nameof(Vin)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    /// <summary>Intervention types this vehicle is allowed to undergo.</summary>
    public ICollection<VehicleInterventionType>? AllowedInterventionTypes { get; set; }
}
