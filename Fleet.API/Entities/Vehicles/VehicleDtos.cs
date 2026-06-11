using System.ComponentModel.DataAnnotations;
using Fleet.API.Entities.Common;
using Fleet.API.Entities.InterventionTypes;

namespace Fleet.API.Entities.Vehicles;

public class VehicleDto
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } = null!;
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public VehicleType VehicleType { get; set; }
    public int Year { get; set; }
    public int Mileage { get; set; }
    public string? Vin { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<VehicleInterventionTypeDto>? AllowedInterventionTypes { get; set; }
}

public class VehicleInterventionTypeDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int InterventionTypeId { get; set; }
    public InterventionTypeDto? InterventionType { get; set; }
}

public class VehicleInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(16)] public string LicensePlate { get; set; } = null!;
    [Required, MaxLength(64)] public string Brand { get; set; } = null!;
    [Required, MaxLength(64)] public string Model { get; set; } = null!;
    public VehicleType VehicleType { get; set; }
    [Range(1950, 2100)] public int Year { get; set; }
    public int Mileage { get; set; }
    [MaxLength(32)] public string? Vin { get; set; }
    public ICollection<VehicleInterventionTypeInputDto>? AllowedInterventionTypes { get; set; }
}

public class VehicleInterventionTypeInputDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int InterventionTypeId { get; set; }
}
