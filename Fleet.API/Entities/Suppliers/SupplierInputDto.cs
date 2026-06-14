using System.ComponentModel.DataAnnotations;

namespace Fleet.API.Entities.Suppliers;

public class SupplierInputDto
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

    /// <summary>Intervention types this supplier is able to perform.</summary>
    public ICollection<SupplierInterventionTypeInputDto>? Capabilities { get; set; }
}

public class SupplierInterventionTypeInputDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public int InterventionTypeId { get; set; }
}
