using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using RoomPlanner.Api.Entities.Floors;

namespace RoomPlanner.Api.Entities.Buildings;

public class Building : IEntityWithSerial, IHasTitle, IHasCode, IHasDescription, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(16)] public string? Code { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    [MaxLength(256)] public string? Address { get; set; }
    [MaxLength(128)] public string? City { get; set; }
    /// <summary>Opening hour (local wall-clock) used by the calendar views.</summary>
    public TimeOnly OpensAt { get; set; } = new(7, 0);
    public TimeOnly ClosesAt { get; set; } = new(20, 0);

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Code), nameof(City), nameof(Address)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<Floor>? Floors { get; set; }
}
