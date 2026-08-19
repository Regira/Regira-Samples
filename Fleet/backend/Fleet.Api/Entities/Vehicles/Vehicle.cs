using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Fleet.Api.Entities.Vehicles;

public class Vehicle : IEntityWithSerial, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(20)] public string LicensePlate { get; set; } = null!;
    [Required, MaxLength(64)] public string Brand { get; set; } = null!;
    [Required, MaxLength(64)] public string Model { get; set; } = null!;
    public VehicleType Type { get; set; }
    public VehicleStatus Status { get; set; } = VehicleStatus.Active;
    public int Year { get; set; }
    public int Mileage { get; set; }
    [MaxLength(32)] public string? Vin { get; set; }
    public DateTime? LastServiceDate { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(LicensePlate), nameof(Brand), nameof(Model), nameof(Vin)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
