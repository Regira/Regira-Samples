using Regira.Entities.QueryBuilders.Abstractions;

namespace RoomPlanner.Api.Entities.Reservations;

public class ReservationQueryBuilder : FilteredQueryBuilderBase<Reservation, int, ReservationSearchObject>
{
    public override IQueryable<Reservation> Build(IQueryable<Reservation> query, ReservationSearchObject? so)
    {
        if (so == null) return query;
        if (so.From.HasValue) query = query.Where(x => x.End > so.From.Value);
        if (so.To.HasValue) query = query.Where(x => x.Start < so.To.Value);
        if (so.RoomId?.Any() == true) query = query.Where(x => x.Rooms!.Any(r => so.RoomId.Contains(r.RoomId)));
        if (so.BuildingId?.Any() == true) query = query.Where(x => x.Rooms!.Any(r => so.BuildingId.Contains(r.Room!.Floor!.BuildingId)));
        if (so.OrganizerId?.Any() == true) query = query.Where(x => so.OrganizerId.Contains(x.OrganizerId));
        if (so.EmployeeId?.Any() == true)
            query = query.Where(x => so.EmployeeId.Contains(x.OrganizerId) || x.Attendees!.Any(a => so.EmployeeId.Contains(a.EmployeeId)));
        if (so.Status?.Any() == true) query = query.Where(x => so.Status.Contains(x.Status));
        if (so.AwaitingApproval.HasValue)
        {
            query = so.AwaitingApproval.Value
                ? query.Where(x => x.Status != ReservationStatus.Cancelled && x.Rooms!.Any(r => r.ApprovalStatus == RoomApprovalStatus.Pending))
                : query.Where(x => !x.Rooms!.Any(r => r.ApprovalStatus == RoomApprovalStatus.Pending));
        }
        return query;
    }
}
