using System.ComponentModel.DataAnnotations;

namespace Fleet.Api.Entities.Suppliers;

public class SupplierInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(256)] public string? ContactEmail { get; set; }
    [MaxLength(32)] public string? ContactPhone { get; set; }
    [MaxLength(256)] public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    // Only include when configured with e.Related(...) — nullable + uninitialized:
    // null = untouched, [] = delete-all, populated = the new set of supported types.
    public ICollection<SupplierInterventionTypeInputDto>? SupportedInterventionTypes { get; set; }
}

public class SupplierInterventionTypeInputDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public int InterventionTypeId { get; set; }
}
