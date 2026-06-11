namespace Fleet.API.Entities.Vehicles;

[Flags]
public enum VehicleIncludes
{
    Default = 0,
    AllowedInterventionTypes = 1 << 0,
    Interventions = 1 << 1,
    All = AllowedInterventionTypes | Interventions
}
