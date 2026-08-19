using Regira.Entities.QueryBuilders.Abstractions;

namespace EventPlanner.Api.Entities.Events;

public class EventQueryBuilder : FilteredQueryBuilderBase<Event, int, EventSearchObject>
{
    public override IQueryable<Event> Build(IQueryable<Event> query, EventSearchObject? so)
    {
        if (so == null) return query;
        if (so.LocationId?.Any() == true) query = query.Where(x => so.LocationId.Contains(x.LocationId));
        if (so.EventCategoryId?.Any() == true) query = query.Where(x => so.EventCategoryId.Contains(x.EventCategoryId));
        if (so.MinStartDate != null) query = query.Where(x => x.StartDate >= so.MinStartDate);
        if (so.MaxStartDate != null) query = query.Where(x => x.StartDate <= so.MaxStartDate);
        if (so.IsFeatured != null) query = query.Where(x => x.IsFeatured == so.IsFeatured);
        return query;
    }
}
