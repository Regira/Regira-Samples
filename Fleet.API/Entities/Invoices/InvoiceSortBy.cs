namespace Fleet.API.Entities.Invoices;

public enum InvoiceSortBy
{
    Default = 0,
    IssueDate,
    IssueDateDesc,
    DueDate,
    DueDateDesc,
    TotalAmount,
    TotalAmountDesc
}
