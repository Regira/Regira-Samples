using System.ComponentModel.DataAnnotations;
using RoomPlanner.Api.Entities.Equipments;
using RoomPlanner.Api.Entities.Floors;

namespace RoomPlanner.Api.Entities.Rooms;

public class RoomDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int FloorId { get; set; }
    public FloorDto? Floor { get; set; }
    public int Capacity { get; set; }
    public bool RequiresApproval { get; set; }
    public bool IsActive { get; set; }
    public string? Color { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<RoomEquipmentDto>? Equipment { get; set; }
}

public class RoomEquipmentDto
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public int EquipmentId { get; set; }
    public EquipmentDto? Equipment { get; set; }
    public int Quantity { get; set; }
}

public class RoomInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(16)] public string? Code { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    public int FloorId { get; set; }
    [Range(1, 1000)] public int Capacity { get; set; }
    public bool RequiresApproval { get; set; }
    public bool IsActive { get; set; } = true;
    [MaxLength(16)] public string? Color { get; set; }
    public ICollection<RoomEquipmentInputDto>? Equipment { get; set; }
}

public class RoomEquipmentInputDto
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public int EquipmentId { get; set; }
    [Range(1, 100)] public int Quantity { get; set; } = 1;
}
