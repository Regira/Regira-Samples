using System.ComponentModel.DataAnnotations;
using Fleet.API.Entities.Interventions;
using Fleet.API.Entities.Invoices;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Fleet.API.Entities.Suppliers;

/// <summary>
/// A garage / service provider that performs interventions on fleet vehicles and sends invoices for them.
/// A supplier can be assigned the intervention types it is able to perform.
/// </summary>
public class Supplier : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasNormalizedContent
{
    public int Id { get; set; }

    /// <summary>Company / trade name of the supplier.</summary>
    [Required, MaxLength(128)]
    public string Title { get; set; } = null!;

    [MaxLength(128)] public string? ContactPerson { get; set; }
    [MaxLength(256)] public string? Email { get; set; }
    [MaxLength(64)] public string? Phone { get; set; }
    [MaxLength(256)] public string? Address { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = new[] { nameof(Title), nameof(ContactPerson), nameof(Email) })]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    /// <summary>Intervention types this supplier is able to perform (many-to-many).</summary>
    public ICollection<SupplierInterventionType>? Capabilities { get; set; }

    public ICollection<Invoice>? Invoices { get; set; }
    public ICollection<Intervention>? Interventions { get; set; }
}
