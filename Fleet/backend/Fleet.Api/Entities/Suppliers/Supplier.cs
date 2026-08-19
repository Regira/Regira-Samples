using System.ComponentModel.DataAnnotations;
using Fleet.Api.Entities.InterventionTypes;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Fleet.Api.Entities.Suppliers;

public class Supplier : IEntityWithSerial, IHasTitle, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    public string? Title { get; set; }
    [MaxLength(256)] public string? ContactEmail { get; set; }
    [MaxLength(32)] public string? ContactPhone { get; set; }
    [MaxLength(256)] public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<SupplierInterventionType>? SupportedInterventionTypes { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(ContactEmail), nameof(Address)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

// Owned m2m join entity — no own .For<>() registration.
public class SupplierInterventionType : IEntityWithSerial
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public int InterventionTypeId { get; set; }
    public InterventionType? InterventionType { get; set; }
}
