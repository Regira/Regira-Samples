using System.ComponentModel.DataAnnotations;

namespace AssetHub.Api.Entities.Assets;

public class AssetWarrantyInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(150)]
    public string Provider { get; set; } = null!;
    [MaxLength(80)]
    public string? WarrantyNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? Cost { get; set; }
    [MaxLength(500)]
    public string? CoverageDetails { get; set; }
}
