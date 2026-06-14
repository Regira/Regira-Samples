using Regira.Entities.Models;

namespace Fleet.API.Entities.Invoices;

public record InvoiceSearchObject : SearchObject
{
    public string? InvoiceNumber { get; set; }
    public ICollection<int>? SupplierId { get; set; }
    public ICollection<InvoiceStatus>? Status { get; set; }
    public DateTime? MinIssueDate { get; set; }
    public DateTime? MaxIssueDate { get; set; }
    public decimal? MinTotalAmount { get; set; }
    public decimal? MaxTotalAmount { get; set; }
}
