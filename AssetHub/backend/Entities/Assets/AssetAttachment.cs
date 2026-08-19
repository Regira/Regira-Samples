using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;

namespace AssetHub.Api.Entities.Assets;

// Owned child of Asset (via e.Related()) -- metadata-only attachment record, no budget slot.
public class AssetAttachment : IEntityWithSerial, ISortable
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public Asset? Asset { get; set; }
    [Required, MaxLength(260)]
    public string FileName { get; set; } = null!;
    [MaxLength(100)]
    public string? ContentType { get; set; }
    public long SizeBytes { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; }
    public int SortOrder { get; set; }
}
