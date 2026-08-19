using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;

namespace AssetHub.Api.Entities.Assets;

// Owned child of Asset (via e.Related()) -- no budget slot.
public class AssetWarranty : IEntityWithSerial
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public Asset? Asset { get; set; }
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
