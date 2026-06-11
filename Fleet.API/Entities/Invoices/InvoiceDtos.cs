using System.ComponentModel.DataAnnotations;
using Fleet.API.Entities.Common;
using Fleet.API.Entities.Suppliers;

namespace Fleet.API.Entities.Invoices;

public class InvoiceDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public int SupplierId { get; set; }
    public SupplierDto? Supplier { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal Amount { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class InvoiceInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(32)] public string? Code { get; set; }
    public int SupplierId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal Amount { get; set; }
}
