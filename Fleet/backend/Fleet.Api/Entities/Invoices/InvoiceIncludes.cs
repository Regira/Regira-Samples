namespace Fleet.Api.Entities.Invoices;

[Flags]
public enum InvoiceIncludes
{
    Default = 0,
    Interventions = 1 << 0,
    All = Interventions
}
