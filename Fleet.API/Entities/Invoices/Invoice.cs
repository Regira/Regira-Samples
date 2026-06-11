using System.ComponentModel.DataAnnotations;
using Fleet.API.Entities.Common;
using Fleet.API.Entities.Interventions;
using Fleet.API.Entities.Suppliers;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Fleet.API.Entities.Invoices;

/// <summary>
/// An invoice sent by a supplier covering one or more interventions it performed.
/// </summary>
public class Invoice : IEntityWithSerial, IHasTimestamps, IHasCode, IHasNormalizedContent
{
    public int Id { get; set; }

    /// <summary>Supplier invoice number.</summary>
    [Required, MaxLength(32)]
    public string? Code { get; set; }

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }

    public InvoiceStatus Status { get; set; }

    /// <summary>Total invoiced amount (sum of the cost of the linked interventions).</summary>
    public decimal Amount { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = new[] { nameof(Code) })]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    /// <summary>Interventions billed on this invoice.</summary>
    public ICollection<Intervention>? Interventions { get; set; }
}
