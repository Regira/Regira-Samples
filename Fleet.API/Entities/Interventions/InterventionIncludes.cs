namespace Fleet.API.Entities.Interventions;

[Flags]
public enum InterventionIncludes
{
    Default = 0,
    Vehicle = 1 << 0,
    InterventionType = 1 << 1,
    Supplier = 1 << 2,
    Invoice = 1 << 3,
    All = Vehicle | InterventionType | Supplier | Invoice
}
