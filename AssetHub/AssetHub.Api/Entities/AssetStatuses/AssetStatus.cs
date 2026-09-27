using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;

namespace AssetHub.Api.Entities.AssetStatuses;

/// <summary>
/// The workflow meaning of a status. Administrators may create any number of statuses;
/// the assign / return actions pick the first status (by SortOrder) of the matching kind.
/// </summary>
public enum StatusKind
{
    Available = 0,
    Assigned = 1,
    Maintenance = 2,
    Inactive = 3
}

public class AssetStatus : IEntityWithSerial, IHasTitle, IHasDescription, ISortable, IHasTimestamps
{
    public int Id { get; set; }
    [Required, MaxLength(48)] public string? Title { get; set; }
    [MaxLength(256)] public string? Description { get; set; }
    /// <summary>Hex color used for the status badge (#RRGGBB)</summary>
    [Required, MaxLength(9)] public string Color { get; set; } = "#6c757d";
    public StatusKind Kind { get; set; }
    public int SortOrder { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public record AssetStatusSearchObject : SearchObject
{
    public ICollection<StatusKind>? Kind { get; set; }
}

public class AssetStatusDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string Color { get; set; } = null!;
    public StatusKind Kind { get; set; }
    public int SortOrder { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class AssetStatusInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(48)] public string? Title { get; set; }
    [MaxLength(256)] public string? Description { get; set; }
    [Required, MaxLength(9), RegularExpression("^#[0-9a-fA-F]{6}$")] public string Color { get; set; } = "#6c757d";
    public StatusKind Kind { get; set; }
    public int SortOrder { get; set; }
}
