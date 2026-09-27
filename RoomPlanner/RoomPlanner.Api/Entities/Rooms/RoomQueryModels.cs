using Regira.Entities.Models;

namespace RoomPlanner.Api.Entities.Rooms;

public record RoomSearchObject : SearchObject
{
    public ICollection<int>? BuildingId { get; set; }
    public ICollection<int>? FloorId { get; set; }
    public int? MinCapacity { get; set; }
    /// <summary>The room must offer ALL of these equipment ids.</summary>
    public ICollection<int>? EquipmentId { get; set; }
    public bool? RequiresApproval { get; set; }
    public bool? IsActive { get; set; }
    /// <summary>With AvailableTo: only rooms without an active (non-cancelled, non-rejected) booking overlapping the period.</summary>
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }
}

public enum RoomSortBy
{
    Default = 0,
    Title,
    TitleDesc,
    Capacity,
    CapacityDesc,
    Building,
}

[Flags]
public enum RoomIncludes
{
    Default = 0,
    Equipment = 1 << 0,
    All = Equipment,
}
