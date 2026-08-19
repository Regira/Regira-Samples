namespace AssetHub.Api.Entities.Assets;

public class AssetWarrantyDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string Provider { get; set; } = null!;
    public string? WarrantyNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? Cost { get; set; }
    public string? CoverageDetails { get; set; }
}
