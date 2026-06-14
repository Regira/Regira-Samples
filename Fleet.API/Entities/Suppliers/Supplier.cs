using System.ComponentModel.DataAnnotations;
using Fleet.API.Entities.InterventionTypes;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Fleet.API.Entities.Suppliers;

/// <summary>
/// A service provider (garage, body shop, tyre centre, ...) that performs
/// interventions and sends invoices for the work performed.
/// </summary>
public class Supplier : IEntityWithSerial, IHasTitle, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Title { get; set; } = null!;

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(32)]
    public string? Phone { get; set; }

    [MaxLength(256)]
    public string? Address { get; set; }

    [MaxLength(128)]
    public string? City { get; set; }

    [MaxLength(32)]
    public string? VatNumber { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(City), nameof(VatNumber)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    /// <summary>Intervention types this supplier is able to perform.</summary>
    public ICollection<SupplierInterventionType>? Capabilities { get; set; }
}
