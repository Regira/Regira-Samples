using Regira.Entities.QueryBuilders.Abstractions;

namespace RoomPlanner.Api.Entities.Reservations;

public class ReservationQueryBuilder : FilteredQueryBuilderBase<Reservation, int, ReservationSearchObject>
{
    public override IQueryable<Reservation> Build(IQueryable<Reservation> query, ReservationSearchObject? so)
    {
        if (so == null)
        {
            return query;
        }

        if (so.OrganizerId?.Any() == true)
        {
            query = query.Where(x => so.OrganizerId.Contains(x.OrganizerId));
        }
        if (so.RoomId?.Any() == true)
        {
            query = query.Where(x => x.Rooms!.Any(r => so.RoomId.Contains(r.RoomId)));
        }
        if (so.FloorId?.Any() == true)
        {
            query = query.Where(x => x.Rooms!.Any(r => so.FloorId.Contains(r.Room!.FloorId)));
        }
        if (so.BuildingId?.Any() == true)
        {
            query = query.Where(x => x.Rooms!.Any(r => so.BuildingId.Contains(r.Room!.Floor!.BuildingId)));
        }
        if (so.Status?.Any() == true)
        {
            query = query.Where(x => so.Status.Contains(x.Status));
        }
        if (so.MinStartTime.HasValue)
        {
            query = query.Where(x => x.StartTime >= so.MinStartTime.Value);
        }
        if (so.MaxStartTime.HasValue)
        {
            query = query.Where(x => x.StartTime <= so.MaxStartTime.Value);
        }
        if (so.MinEndTime.HasValue)
        {
            query = query.Where(x => x.EndTime >= so.MinEndTime.Value);
        }
        if (so.MaxEndTime.HasValue)
        {
            query = query.Where(x => x.EndTime <= so.MaxEndTime.Value);
        }

        return query;
    }
}
