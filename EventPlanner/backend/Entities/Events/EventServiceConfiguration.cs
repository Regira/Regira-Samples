using EventPlanner.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;

namespace EventPlanner.Api.Entities.Events;

public static class EventServiceConfiguration
{
    // Complex registration (typed sort + includes) — 1/2 complex budget slot
    public static EntityServiceCollection<EventPlannerDbContext> AddEvents(this IEntityServiceCollection<EventPlannerDbContext> services)
        => services.For<Event, EventSearchObject, EventSortBy, EventIncludes>(e =>
        {
            e.AddFilter<EventQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                EventSortBy.Title => query.OrderOrThenBy(x => x.Title),
                EventSortBy.TitleDesc => query.OrderOrThenByDescending(x => x.Title),
                EventSortBy.StartDate => query.OrderOrThenBy(x => x.StartDate),
                EventSortBy.StartDateDesc => query.OrderOrThenByDescending(x => x.StartDate),
                _ => query.OrderOrThenBy(x => x.StartDate)
            });
            // Location + EventCategory are to-one references shown on every list row: unconditional.
            // Sessions is a collection: gated behind the flag, split query since it joins alongside Location/Category.
            e.Includes((query, includes) =>
            {
                query = query.Include(x => x.Location!).Include(x => x.EventCategory!);
                if (includes?.HasFlag(EventIncludes.Sessions) == true)
                    query = query.Include(x => x.Sessions!.OrderBy(s => s.StartTime)).AsSplitQuery();
                return query;
            });
            e.AddProcessor<EventProcessor>();
        });
}
