using System.ComponentModel.DataAnnotations;
using Fleet.Api.Entities.Interventions;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Fleet.Api.Entities.Vehicles;

public enum VehicleType { Car = 0, Van, Truck, Bus, Motorcycle, Trailer }
public enum FuelType { Petrol = 0, Diesel, Electric, Hybrid, Lpg, Cng, None }
public enum VehicleStatus { Active = 0, InMaintenance, OutOfService, Retired }

public class Vehicle : IEntityWithSerial, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(16)] public string LicensePlate { get; set; } = null!;
    [MaxLength(17)] public string? Vin { get; set; }
    [Required, MaxLength(64)] public string Make { get; set; } = null!;
    [Required, MaxLength(64)] public string Model { get; set; } = null!;
    public int Year { get; set; }
    public VehicleType VehicleType { get; set; }
    public FuelType FuelType { get; set; }
    public VehicleStatus Status { get; set; }
    public int Mileage { get; set; }
    [MaxLength(64)] public string? Department { get; set; }
    [MaxLength(64)] public string? AssignedDriver { get; set; }
    public DateOnly? AcquisitionDate { get; set; }
    public DateOnly? NextServiceDate { get; set; }
    [MaxLength(1024)] public string? Notes { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(LicensePlate), nameof(Vin), nameof(Make), nameof(Model), nameof(Department), nameof(AssignedDriver)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<Intervention>? Interventions { get; set; }
}
