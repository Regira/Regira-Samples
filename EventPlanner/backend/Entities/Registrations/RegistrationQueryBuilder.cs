using Regira.Entities.QueryBuilders.Abstractions;

namespace EventPlanner.Api.Entities.Registrations;

public class RegistrationQueryBuilder : FilteredQueryBuilderBase<Registration, int, RegistrationSearchObject>
{
    public override IQueryable<Registration> Build(IQueryable<Registration> query, RegistrationSearchObject? so)
    {
        if (so == null) return query;
        if (so.EmployeeId?.Any() == true) query = query.Where(x => so.EmployeeId.Contains(x.EmployeeId));
        if (so.EventId?.Any() == true) query = query.Where(x => so.EventId.Contains(x.EventId));
        if (so.SessionId?.Any() == true) query = query.Where(x => x.SelectedSessions!.Any(ss => so.SessionId.Contains(ss.SessionId)));
        if (so.Status?.Any() == true) query = query.Where(x => so.Status.Contains(x.Status));
        return query;
    }
}
