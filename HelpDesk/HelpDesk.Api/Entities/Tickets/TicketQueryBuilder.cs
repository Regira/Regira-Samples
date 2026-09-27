using HelpDesk.Api.Infrastructure.Security;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.QueryBuilders.Abstractions;

namespace HelpDesk.Api.Entities.Tickets;

public class TicketQueryBuilder(CurrentUser currentUser) : FilteredQueryBuilderBase<Ticket, int, TicketSearchObject>
{
    public override IQueryable<Ticket> Build(IQueryable<Ticket> query, TicketSearchObject? so)
    {
        if (so == null) return query;

        if (so.StatusId?.Any() == true) query = query.Where(x => so.StatusId.Contains(x.StatusId));
        if (so.PriorityId?.Any() == true) query = query.Where(x => so.PriorityId.Contains(x.PriorityId));
        if (so.CategoryId?.Any() == true) query = query.Where(x => x.Categories!.Any(c => so.CategoryId.Contains(c.CategoryId)));
        if (so.SupportTeamId?.Any() == true) query = query.Where(x => x.SupportTeamId != null && so.SupportTeamId.Contains(x.SupportTeamId.Value));
        if (so.AssignedEmployeeId?.Any() == true) query = query.Where(x => x.AssignedEmployeeId != null && so.AssignedEmployeeId.Contains(x.AssignedEmployeeId.Value));
        if (so.CustomerId?.Any() == true) query = query.Where(x => so.CustomerId.Contains(x.CustomerId));

        if (so.IsClosed.HasValue) query = query.Where(x => x.Status!.IsClosed == so.IsClosed.Value);
        if (so.IsAssigned.HasValue) query = so.IsAssigned.Value ? query.Where(x => x.AssignedEmployeeId != null) : query.Where(x => x.AssignedEmployeeId == null);
        if (so.AssignedToMe == true)
        {
            var userId = currentUser.UserId;
            query = userId == null ? query.Where(_ => false) : query.Where(x => x.AssignedEmployee!.UserId == userId);
        }
        if (so.IsOverdue.HasValue)
        {
            var now = DateTime.UtcNow;
            query = so.IsOverdue.Value
                ? query.Where(x => !x.Status!.IsClosed && x.DueDate != null && x.DueDate < now)
                : query.Where(x => x.Status!.IsClosed || x.DueDate == null || x.DueDate >= now);
        }
        if (so.HasAttachment.HasValue) query = query.FilterHasAttachment(so.HasAttachment);

        return query;
    }
}
