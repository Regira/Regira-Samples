namespace Fleet.API.Entities.Invoices;

[Flags]
public enum InvoiceIncludes
{
    Default = 0,
    Supplier = 1 << 0,
    Interventions = 1 << 1,
    All = Supplier | Interventions
}
