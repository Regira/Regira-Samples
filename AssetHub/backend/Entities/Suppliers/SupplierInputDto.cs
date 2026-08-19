using System.ComponentModel.DataAnnotations;

namespace AssetHub.Api.Entities.Suppliers;

public class SupplierInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(150)]
    public string Title { get; set; } = null!;
    [MaxLength(100)]
    public string? ContactName { get; set; }
    [MaxLength(150)]
    public string? Email { get; set; }
    [MaxLength(30)]
    public string? Phone { get; set; }
    [MaxLength(200)]
    public string? Website { get; set; }
}
