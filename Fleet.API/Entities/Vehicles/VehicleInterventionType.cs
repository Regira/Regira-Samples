using Fleet.API.Entities.InterventionTypes;
using Regira.Entities.Models.Abstractions;

namespace Fleet.API.Entities.Vehicles;

/// <summary>
/// Join entity linking a <see cref="Vehicle"/> to an <see cref="InterventionType"/>
/// it is allowed to undergo. Owned by the vehicle (managed via <c>Related()</c>).
/// </summary>
public class VehicleInterventionType : IEntityWithSerial
{
    public int Id { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public int InterventionTypeId { get; set; }
    public InterventionType? InterventionType { get; set; }
}
