using Regira.Entities.Models;

namespace RoomPlanner.Api.Entities.Reservations;

public record ReservationSearchObject : SearchObject
{
    public ICollection<int>? OrganizerId { get; set; }
    public ICollection<int>? RoomId { get; set; }
    public ICollection<int>? FloorId { get; set; }
    public ICollection<int>? BuildingId { get; set; }
    public ICollection<ReservationStatus>? Status { get; set; }
    public DateTime? MinStartTime { get; set; }
    public DateTime? MaxStartTime { get; set; }
    public DateTime? MinEndTime { get; set; }
    public DateTime? MaxEndTime { get; set; }
}
