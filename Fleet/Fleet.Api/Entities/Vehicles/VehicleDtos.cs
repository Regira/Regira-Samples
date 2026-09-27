using System.ComponentModel.DataAnnotations;

namespace Fleet.Api.Entities.Vehicles;

public class VehicleDto
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } = null!;
    public string? Vin { get; set; }
    public string Make { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public VehicleType VehicleType { get; set; }
    public FuelType FuelType { get; set; }
    public VehicleStatus Status { get; set; }
    public int Mileage { get; set; }
    public string? Department { get; set; }
    public string? AssignedDriver { get; set; }
    public DateOnly? AcquisitionDate { get; set; }
    public DateOnly? NextServiceDate { get; set; }
    public string? Notes { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class VehicleInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(16)] public string LicensePlate { get; set; } = null!;
    [MaxLength(17)] public string? Vin { get; set; }
    [Required, MaxLength(64)] public string Make { get; set; } = null!;
    [Required, MaxLength(64)] public string Model { get; set; } = null!;
    [Range(1950, 2100)] public int Year { get; set; }
    public VehicleType VehicleType { get; set; }
    public FuelType FuelType { get; set; }
    public VehicleStatus Status { get; set; }
    [Range(0, 5_000_000)] public int Mileage { get; set; }
    [MaxLength(64)] public string? Department { get; set; }
    [MaxLength(64)] public string? AssignedDriver { get; set; }
    public DateOnly? AcquisitionDate { get; set; }
    public DateOnly? NextServiceDate { get; set; }
    [MaxLength(1024)] public string? Notes { get; set; }
}
