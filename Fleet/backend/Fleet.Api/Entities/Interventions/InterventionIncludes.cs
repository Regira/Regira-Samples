namespace Fleet.Api.Entities.Interventions;

[Flags]
public enum InterventionIncludes
{
    Default = 0,
    InterventionTypes = 1 << 0,
    All = InterventionTypes
}
