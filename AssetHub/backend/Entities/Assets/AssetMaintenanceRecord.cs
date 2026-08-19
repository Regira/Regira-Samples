using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;

namespace AssetHub.Api.Entities.Assets;

// Owned child of Asset (via e.Related()) -- no budget slot.
public class AssetMaintenanceRecord : IEntityWithSerial
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public Asset? Asset { get; set; }
    public DateTime MaintenanceDate { get; set; }
    [Required, MaxLength(150)]
    public string PerformedBy { get; set; } = null!;
    [Required, MaxLength(500)]
    public string Description { get; set; } = null!;
    public decimal? Cost { get; set; }
    public DateTime? NextDueDate { get; set; }
}
