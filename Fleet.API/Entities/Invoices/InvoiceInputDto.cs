using System.ComponentModel.DataAnnotations;

namespace Fleet.API.Entities.Invoices;

public class InvoiceInputDto
{
    public int Id { get; set; }

    [Required, MaxLength(32)]
    public string InvoiceNumber { get; set; } = null!;

    public int SupplierId { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
}
