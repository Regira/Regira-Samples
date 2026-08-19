using Fleet.Api.Entities.InterventionTypes;

namespace Fleet.Api.Entities.Suppliers;

public class SupplierDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public ICollection<SupplierInterventionTypeDto>? SupportedInterventionTypes { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class SupplierInterventionTypeDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public int InterventionTypeId { get; set; }
    public InterventionTypeDto? InterventionType { get; set; }
}
