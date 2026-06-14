using Fleet.API.Entities.InterventionTypes;

namespace Fleet.API.Entities.Suppliers;

/// <summary>Lightweight supplier projection for nested references.</summary>
public class SupplierCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
}

public class SupplierDto : SupplierCoreDto
{
    public string? Address { get; set; }
    public string? VatNumber { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<SupplierInterventionTypeDto>? Capabilities { get; set; }
}

public class SupplierInterventionTypeDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public int InterventionTypeId { get; set; }
    public InterventionTypeDto? InterventionType { get; set; }
}
