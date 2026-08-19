namespace AssetHub.Api.Entities.Assets;

public class AssetMaintenanceRecordDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public string PerformedBy { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal? Cost { get; set; }
    public DateTime? NextDueDate { get; set; }
}
