using System.ComponentModel.DataAnnotations;
using Fleet.API.Entities.Interventions;
using Fleet.API.Entities.Suppliers;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Fleet.API.Entities.Invoices;

/// <summary>
/// An invoice sent by a <see cref="Supplier"/> covering one or more interventions.
/// </summary>
public class Invoice : IEntityWithSerial, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }

    [Required, MaxLength(32)]
    public string InvoiceNumber { get; set; } = null!;

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }

    public InvoiceStatus Status { get; set; }

    public decimal TotalAmount { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(InvoiceNumber)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    /// <summary>Interventions covered by this invoice.</summary>
    public ICollection<Intervention>? Interventions { get; set; }
}
