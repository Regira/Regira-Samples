using Fleet.API.Entities.Interventions;
using Fleet.API.Entities.Suppliers;

namespace Fleet.API.Entities.Invoices;

/// <summary>Lightweight invoice projection for nested references.</summary>
public class InvoiceCoreDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
}

public class InvoiceDto : InvoiceCoreDto
{
    public int SupplierId { get; set; }
    public SupplierCoreDto? Supplier { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<InterventionCoreDto>? Interventions { get; set; }
}
