using System.ComponentModel.DataAnnotations;
using Fleet.API.Entities.InterventionTypes;

namespace Fleet.API.Entities.Suppliers;

public class SupplierDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
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

public class SupplierInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(128)] public string? ContactPerson { get; set; }
    [MaxLength(256), EmailAddress] public string? Email { get; set; }
    [MaxLength(64)] public string? Phone { get; set; }
    [MaxLength(256)] public string? Address { get; set; }
    public ICollection<SupplierInterventionTypeInputDto>? Capabilities { get; set; }
}

public class SupplierInterventionTypeInputDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public int InterventionTypeId { get; set; }
}
