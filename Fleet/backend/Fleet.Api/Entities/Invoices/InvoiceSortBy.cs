namespace Fleet.Api.Entities.Invoices;

public enum InvoiceSortBy
{
    Default = 0,
    IssueDate,
    IssueDateDesc,
    DueDate,
    Status,
    TotalAmount,
    TotalAmountDesc
}
