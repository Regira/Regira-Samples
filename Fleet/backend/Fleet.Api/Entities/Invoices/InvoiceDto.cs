using Fleet.Api.Entities.Interventions;
using Fleet.Api.Entities.Suppliers;

namespace Fleet.Api.Entities.Invoices;

public class InvoiceDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public int SupplierId { get; set; }
    public SupplierDto? Supplier { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public ICollection<InterventionDto>? Interventions { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
