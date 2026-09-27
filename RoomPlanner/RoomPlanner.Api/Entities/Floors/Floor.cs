using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using RoomPlanner.Api.Entities.Buildings;
using RoomPlanner.Api.Entities.Rooms;

namespace RoomPlanner.Api.Entities.Floors;

public class Floor : IEntityWithSerial, IHasTitle, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    public int BuildingId { get; set; }
    public Building? Building { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    /// <summary>Storey number: 0 = ground floor, negative = basement.</summary>
    public int Level { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<Room>? Rooms { get; set; }
}

public record FloorSearchObject : SearchObject
{
    public ICollection<int>? BuildingId { get; set; }
}

public class FloorDto
{
    public int Id { get; set; }
    public int BuildingId { get; set; }
    public BuildingDto? Building { get; set; }
    public string Title { get; set; } = null!;
    public int Level { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class FloorInputDto
{
    public int Id { get; set; }
    public int BuildingId { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    public int Level { get; set; }
}
