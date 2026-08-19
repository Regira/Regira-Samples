using Regira.Entities.Models;

namespace Fleet.Api.Entities.Invoices;

public record InvoiceSearchObject : SearchObject
{
    public string? Code { get; set; }
    public ICollection<int>? SupplierId { get; set; }
    public ICollection<InvoiceStatus>? Status { get; set; }
    public DateTime? MinIssueDate { get; set; }
    public DateTime? MaxIssueDate { get; set; }
    public DateTime? MinDueDate { get; set; }
    public DateTime? MaxDueDate { get; set; }
}
