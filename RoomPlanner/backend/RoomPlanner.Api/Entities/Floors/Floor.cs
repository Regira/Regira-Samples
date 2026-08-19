using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using RoomPlanner.Api.Entities.Buildings;

namespace RoomPlanner.Api.Entities.Floors;

public class Floor : IEntityWithSerial, IHasTimestamps, IHasTitle
{
    public int Id { get; set; }

    public int BuildingId { get; set; }
    public Building? Building { get; set; }

    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;

    /// <summary>Numeric floor level used for ordering (0 = ground floor).</summary>
    public int Level { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
