using System.ComponentModel.DataAnnotations;

namespace Fleet.Api.Entities.Vehicles;

public class VehicleInputDto
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
}
