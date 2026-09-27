using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace RoomPlanner.Api.Entities.Equipments;

/// <summary>A kind of equipment a meeting room can offer (projector, whiteboard, video conferencing, ...).</summary>
public class Equipment : IEntityWithSerial, IHasTitle, IHasCode, IHasDescription, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(32)] public string? Code { get; set; }
    [MaxLength(512)] public string? Description { get; set; }
    /// <summary>Icon name used by the SPA (a bootstrap-icons name without the "bi-" prefix).</summary>
    [MaxLength(64)] public string? Icon { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Code), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class EquipmentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class EquipmentInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(32)] public string? Code { get; set; }
    [MaxLength(512)] public string? Description { get; set; }
    [MaxLength(64)] public string? Icon { get; set; }
}
