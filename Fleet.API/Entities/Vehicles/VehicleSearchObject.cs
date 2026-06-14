using Regira.Entities.Models;

namespace Fleet.API.Entities.Vehicles;

public record VehicleSearchObject : SearchObject
{
    public string? LicensePlate { get; set; }
    public string? Brand { get; set; }
    public ICollection<VehicleType>? VehicleType { get; set; }
    public int? MinMileage { get; set; }
    public int? MaxMileage { get; set; }
    /// <summary>Only return vehicles allowed to undergo any of these intervention types.</summary>
    public ICollection<int>? InterventionTypeId { get; set; }
}
