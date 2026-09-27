using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using RoomPlanner.Api.Entities.Equipments;
using RoomPlanner.Api.Entities.Floors;
using RoomPlanner.Api.Entities.Reservations;

namespace RoomPlanner.Api.Entities.Rooms;

public class Room : IEntityWithSerial, IHasTitle, IHasCode, IHasDescription, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(16)] public string? Code { get; set; }
    [MaxLength(1024)] public string? Description { get; set; }
    public int FloorId { get; set; }
    public Floor? Floor { get; set; }
    /// <summary>Maximum number of people (organizer included).</summary>
    public int Capacity { get; set; }
    /// <summary>When true, a reservation of this room stays Pending until a facility manager approves it.</summary>
    public bool RequiresApproval { get; set; }
    /// <summary>Out-of-service rooms can't be booked.</summary>
    public bool IsActive { get; set; } = true;
    /// <summary>Hex colour used by the timeline views.</summary>
    [MaxLength(16)] public string? Color { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Code), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    /// <summary>Owned join rows (m2m with Equipment), synced through Related().</summary>
    public ICollection<RoomEquipment>? Equipment { get; set; }
    /// <summary>Back-reference only (used by the availability filter) - never mapped to a DTO.</summary>
    public ICollection<ReservationRoom>? Bookings { get; set; }
}

public class RoomEquipment : IEntityWithSerial
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public Room? Room { get; set; }
    public int EquipmentId { get; set; }
    public Equipment? Equipment { get; set; }
    /// <summary>Number of units (e.g. 2 screens).</summary>
    public int Quantity { get; set; } = 1;
}
