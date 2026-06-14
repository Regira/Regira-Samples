using System.ComponentModel.DataAnnotations;

namespace Fleet.API.Entities.Vehicles;

public class VehicleInputDto
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
    public int Mileage { get; set; }

    /// <summary>Intervention types this vehicle is allowed to undergo.</summary>
    public ICollection<VehicleInterventionTypeInputDto>? AllowedInterventionTypes { get; set; }
}

public class VehicleInterventionTypeInputDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int InterventionTypeId { get; set; }
}
