using Regira.Entities.QueryBuilders.Abstractions;

namespace HelpDesk.API.Entities.Tickets;

public class TicketQueryBuilder : FilteredQueryBuilderBase<Ticket, int, TicketSearchObject>
{
    public override IQueryable<Ticket> Build(IQueryable<Ticket> query, TicketSearchObject? so)
    {
        if (so == null) return query;

        if (so.CategoryId?.Any() == true)
            query = query.Where(x => x.Categories!.Any(tc => so.CategoryId.Contains(tc.CategoryId)));
        if (so.PriorityId?.Any() == true)
            query = query.Where(x => so.PriorityId.Contains(x.PriorityId));
        if (so.StatusId?.Any() == true)
            query = query.Where(x => so.StatusId.Contains(x.StatusId));
        if (so.SupportTeamId?.Any() == true)
            query = query.Where(x => x.SupportTeamId != null && so.SupportTeamId.Contains(x.SupportTeamId.Value));
        if (so.CustomerId?.Any() == true)
            query = query.Where(x => so.CustomerId.Contains(x.CustomerId));
        if (so.AssignedEmployeeId?.Any() == true)
            query = query.Where(x => x.AssignedEmployeeId != null && so.AssignedEmployeeId.Contains(x.AssignedEmployeeId.Value));
        if (so.IsUnassigned == true)
            query = query.Where(x => x.AssignedEmployeeId == null);
        if (so.IsClosed != null)
            query = query.Where(x => x.Status!.IsClosed == so.IsClosed);

        return query;
    }
}
