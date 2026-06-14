namespace Fleet.API.Entities.Interventions;

[Flags]
public enum InterventionIncludes
{
    Default = 0,
    Vehicle = 1 << 0,
    Supplier = 1 << 1,
    Type = 1 << 2,
    Invoice = 1 << 3,
    All = Vehicle | Supplier | Type | Invoice
}
