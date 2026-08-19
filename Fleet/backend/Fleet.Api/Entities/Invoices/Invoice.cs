using System.ComponentModel.DataAnnotations;
using Fleet.Api.Entities.Interventions;
using Fleet.Api.Entities.Suppliers;
using Regira.Entities.Models.Abstractions;

namespace Fleet.Api.Entities.Invoices;

public class Invoice : IEntityWithSerial, IHasTimestamps, IHasCode
{
    public int Id { get; set; }
    [MaxLength(32)] public string? Code { get; set; }

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }

    // Server-owned — aggregated from Interventions pointing at this invoice via InvoiceId (see
    // InterventionInvoiceTotalPrepper). Never trust client input for this.
    public decimal TotalAmount { get; set; }

    // Independent entity with a back-ref collection — loaded via Include(), NOT e.Related().
    // Each Intervention owns its own InvoiceId write.
    public ICollection<Intervention>? Interventions { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
