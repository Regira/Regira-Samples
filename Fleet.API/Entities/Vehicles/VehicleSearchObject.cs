using Fleet.API.Entities.Common;
using Regira.Entities.Models;

namespace Fleet.API.Entities.Vehicles;

public record VehicleSearchObject : SearchObject
{
    public ICollection<VehicleType>? VehicleType { get; set; }
    public string? Brand { get; set; }

    /// <summary>Filter on vehicles allowed to undergo any of these intervention types.</summary>
    public ICollection<int>? InterventionTypeId { get; set; }

    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
}
