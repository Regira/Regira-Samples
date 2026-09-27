using System.ComponentModel.DataAnnotations;
using Fleet.Api.Entities.InterventionTypes;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Fleet.Api.Entities.Suppliers;

public class Supplier : IEntityWithSerial, IHasTitle, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string? Title { get; set; }
    [MaxLength(32)] public string? VatNumber { get; set; }
    [MaxLength(128)] public string? ContactPerson { get; set; }
    [MaxLength(128)] public string? Email { get; set; }
    [MaxLength(32)] public string? Phone { get; set; }
    [MaxLength(128)] public string? Street { get; set; }
    [MaxLength(16)] public string? PostalCode { get; set; }
    [MaxLength(64)] public string? City { get; set; }
    /// <summary>Supplier rating 1-5 (null = not rated).</summary>
    public int? Rating { get; set; }
    public bool IsActive { get; set; } = true;
    [MaxLength(1024)] public string? Notes { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(VatNumber), nameof(ContactPerson), nameof(City)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    /// <summary>Intervention types this supplier is able to perform (owned join rows, synced via Related()).</summary>
    public ICollection<SupplierInterventionType>? InterventionTypes { get; set; }
}

/// <summary>Owned m2m join row: Supplier -> InterventionType capability.</summary>
public class SupplierInterventionType : IEntityWithSerial
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public int InterventionTypeId { get; set; }
    public InterventionType? InterventionType { get; set; }
}

public class SupplierDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? VatNumber { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public int? Rating { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<SupplierInterventionTypeDto>? InterventionTypes { get; set; }
}

public class SupplierInterventionTypeDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public int InterventionTypeId { get; set; }
    public InterventionTypeDto? InterventionType { get; set; }
}

public class SupplierInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string? Title { get; set; }
    [MaxLength(32)] public string? VatNumber { get; set; }
    [MaxLength(128)] public string? ContactPerson { get; set; }
    [MaxLength(128), EmailAddress] public string? Email { get; set; }
    [MaxLength(32)] public string? Phone { get; set; }
    [MaxLength(128)] public string? Street { get; set; }
    [MaxLength(16)] public string? PostalCode { get; set; }
    [MaxLength(64)] public string? City { get; set; }
    [Range(1, 5)] public int? Rating { get; set; }
    public bool IsActive { get; set; } = true;
    [MaxLength(1024)] public string? Notes { get; set; }
    // nullable + uninitialized: omitted = untouched, [] = remove all capabilities
    public ICollection<SupplierInterventionTypeInputDto>? InterventionTypes { get; set; }
}

public class SupplierInterventionTypeInputDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public int InterventionTypeId { get; set; }
}

public record SupplierSearchObject : SearchObject
{
    /// <summary>Suppliers able to perform ANY of these intervention types.</summary>
    public ICollection<int>? InterventionTypeId { get; set; }
    public bool? IsActive { get; set; }
    public string? City { get; set; }
}
