using System.ComponentModel.DataAnnotations;

namespace AssetHub.Api.Entities.Assets;

public class AssetMaintenanceRecordInputDto
{
    public int Id { get; set; }
    public DateTime MaintenanceDate { get; set; }
    [Required, MaxLength(150)]
    public string PerformedBy { get; set; } = null!;
    [Required, MaxLength(500)]
    public string Description { get; set; } = null!;
    public decimal? Cost { get; set; }
    public DateTime? NextDueDate { get; set; }
}
