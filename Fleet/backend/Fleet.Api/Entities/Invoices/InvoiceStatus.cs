namespace Fleet.Api.Entities.Invoices;

public enum InvoiceStatus
{
    Draft = 0,
    Sent,
    Paid,
    Overdue,
    Cancelled
}
