using EventPlanner.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;

namespace EventPlanner.Api.Entities.Events;

public static class EventServiceConfiguration
{
    public static EntityServiceCollection<EventPlannerDbContext> AddEvents(this IEntityServiceCollection<EventPlannerDbContext> services)
        => services.For<Event, EventSearchObject, EventSortBy, EventIncludes>(e =>
        {
            e.AddFilter<EventQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                EventSortBy.StartDateDesc => query.OrderOrThenByDescending(x => x.StartDate),
                EventSortBy.Title => query.OrderOrThenBy(x => x.Title),
                EventSortBy.Created => query.OrderOrThenByDescending(x => x.Created),
                _ => query.OrderOrThenBy(x => x.StartDate)
            });
            // one Includes registration: to-one references shown on every row load unconditionally,
            // the sessions (+ speakers) collection only when asked for (always on Details)
            e.Includes((query, includes) =>
            {
                query = query.Include(x => x.Category).Include(x => x.Location);
                if (includes?.HasFlag(EventIncludes.Sessions) == true)
                    query = query
                        .Include(x => x.Sessions!.OrderBy(s => s.StartTime))
                            .ThenInclude(s => s.Speakers!)
                                .ThenInclude(sp => sp.Speaker)
                        .AsSplitQuery();
                return query;
            });
            // validation + speaker dedupe must run BEFORE the Related() sync (preppers run in registration order)
            e.AddPrepper<EventPrepper>();
            // owned sessions, each owning its speaker join rows
            e.Related(x => x.Sessions, configure: session => session.Related(s => s.Speakers));
            e.AddProcessor<EventProcessor>();
        });
}
