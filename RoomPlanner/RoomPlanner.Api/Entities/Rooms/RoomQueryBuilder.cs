using Regira.Entities.QueryBuilders.Abstractions;
using RoomPlanner.Api.Entities.Reservations;

namespace RoomPlanner.Api.Entities.Rooms;

public class RoomQueryBuilder : FilteredQueryBuilderBase<Room, int, RoomSearchObject>
{
    public override IQueryable<Room> Build(IQueryable<Room> query, RoomSearchObject? so)
    {
        if (so == null) return query;
        if (so.BuildingId?.Any() == true) query = query.Where(x => so.BuildingId.Contains(x.Floor!.BuildingId));
        if (so.FloorId?.Any() == true) query = query.Where(x => so.FloorId.Contains(x.FloorId));
        if (so.MinCapacity.HasValue) query = query.Where(x => x.Capacity >= so.MinCapacity.Value);
        if (so.EquipmentId?.Any() == true)
        {
            // AND semantics: one correlated EXISTS per requested equipment
            foreach (var equipmentId in so.EquipmentId.Distinct().ToList())
                query = query.Where(x => x.Equipment!.Any(re => re.EquipmentId == equipmentId));
        }
        if (so.RequiresApproval.HasValue) query = query.Where(x => x.RequiresApproval == so.RequiresApproval.Value);
        if (so.IsActive.HasValue) query = query.Where(x => x.IsActive == so.IsActive.Value);
        if (so.AvailableFrom.HasValue && so.AvailableTo.HasValue)
        {
            var from = so.AvailableFrom.Value;
            var to = so.AvailableTo.Value;
            query = query.Where(x => !x.Bookings!.Any(b =>
                b.ApprovalStatus != RoomApprovalStatus.Rejected
                && b.Reservation!.Status != ReservationStatus.Cancelled
                && b.Reservation.Start < to && b.Reservation.End > from));
        }
        return query;
    }
}
