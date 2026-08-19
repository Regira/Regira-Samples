using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;

namespace AssetHub.Api.Entities.AssetStatuses;

// Reference data (Asset.StatusId is a required FK) -> intentionally NOT IArchivable.
public class AssetStatus : IEntityWithSerial, IHasTitle, IHasTimestamps
{
    public int Id { get; set; }
    [Required, MaxLength(50)]
    public string Title { get; set; } = null!;
    /// <summary>Hex color used for status indicators in the UI, e.g. "#22c55e".</summary>
    [Required, MaxLength(9)]
    public string ColorHex { get; set; } = "#64748b";
    /// <summary>Whether an asset in this status is considered usable/deployable.</summary>
    public bool IsOperational { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
