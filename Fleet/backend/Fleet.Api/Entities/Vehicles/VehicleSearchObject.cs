using Regira.Entities.Models;

namespace Fleet.Api.Entities.Vehicles;

public record VehicleSearchObject : SearchObject
{
    public ICollection<VehicleType>? Type { get; set; }
    public ICollection<VehicleStatus>? Status { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
}
