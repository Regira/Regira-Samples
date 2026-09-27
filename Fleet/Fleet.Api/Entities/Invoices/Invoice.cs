using System.ComponentModel.DataAnnotations;
using Fleet.Api.Entities.Interventions;
using Fleet.Api.Entities.Suppliers;
using Fleet.Api.Entities.Vehicles;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Fleet.Api.Entities.Invoices;

public enum InvoiceStatus { Received = 0, Approved, Paid, Disputed }

public class Invoice : IEntityWithSerial, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    /// <summary>The supplier's own invoice number (unique per supplier).</summary>
    [Required, MaxLength(32)] public string InvoiceNumber { get; set; } = null!;
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateOnly? PaidDate { get; set; }
    /// <summary>VAT percentage, e.g. 21.</summary>
    public decimal VatRate { get; set; } = 21m;
    // amounts are derived from the linked interventions by the invoice prepper - never client input
    public decimal SubTotal { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    [MaxLength(1024)] public string? Notes { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(InvoiceNumber)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    /// <summary>Interventions billed on this invoice (NOT owned: each intervention writes its own InvoiceId).</summary>
    public ICollection<Intervention>? Interventions { get; set; }
}

public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public int SupplierId { get; set; }
    public SupplierDto? Supplier { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateOnly? PaidDate { get; set; }
    public decimal VatRate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<InvoiceInterventionDto>? Interventions { get; set; }
}

/// <summary>Intervention as listed on an invoice (no back-reference to the invoice).</summary>
public class InvoiceInterventionDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public VehicleDto? Vehicle { get; set; }
    public InterventionStatus Status { get; set; }
    public DateOnly ScheduledDate { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public string? Description { get; set; }
    public decimal TotalCost { get; set; }
    public ICollection<InterventionLineDto>? Lines { get; set; }
}

public class InvoiceInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(32)] public string InvoiceNumber { get; set; } = null!;
    [Range(1, int.MaxValue, ErrorMessage = "Supplier is required.")] public int SupplierId { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateOnly? PaidDate { get; set; }
    [Range(0, 100)] public decimal VatRate { get; set; } = 21m;
    [MaxLength(1024)] public string? Notes { get; set; }
}

public record InvoiceSearchObject : SearchObject
{
    public ICollection<int>? SupplierId { get; set; }
    public ICollection<InvoiceStatus>? Status { get; set; }
    /// <summary>Unpaid and past its due date.</summary>
    public bool? IsOverdue { get; set; }
    public DateOnly? MinDate { get; set; }
    public DateOnly? MaxDate { get; set; }
}
