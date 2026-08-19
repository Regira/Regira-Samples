using System.ComponentModel.DataAnnotations;

namespace Fleet.Api.Entities.Invoices;

public class InvoiceInputDto
{
    public int Id { get; set; }
    // Code: server-generated (InvoiceCodePrimer), TotalAmount: server-owned (aggregated) -> both excluded.
    [Required] public int SupplierId { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
}
