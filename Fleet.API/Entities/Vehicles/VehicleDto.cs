using Fleet.API.Entities.InterventionTypes;

namespace Fleet.API.Entities.Vehicles;

/// <summary>Lightweight vehicle projection for nested references.</summary>
public class VehicleCoreDto
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } = null!;
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public VehicleType VehicleType { get; set; }
}

public class VehicleDto : VehicleCoreDto
{
    public string? Vin { get; set; }
    public int? Year { get; set; }
    public int Mileage { get; set; }
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
