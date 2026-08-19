using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using RoomPlanner.Api.Entities.Floors;

namespace RoomPlanner.Api.Entities.MeetingRooms;

public class MeetingRoom : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasNormalizedContent
{
    public int Id { get; set; }

    public int FloorId { get; set; }
    public Floor? Floor { get; set; }

    /// <summary>Room name, e.g. "Orion".</summary>
    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;

    public int Capacity { get; set; }

    public RoomEquipment Equipment { get; set; } = RoomEquipment.None;

    /// <summary>When true, reservations for this room stay Pending until manually approved.</summary>
    public bool RequiresApproval { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
