using EventPlanner.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace EventPlanner.Api.Entities.Sessions;

public static class SessionServiceConfiguration
{
    // Simple registration (custom SearchObject, no typed sort/includes) — 5/5 simple budget slot
    public static EntityServiceCollection<EventPlannerDbContext> AddSessions(this IEntityServiceCollection<EventPlannerDbContext> services)
        => services.For<Session, int, SessionSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so?.EventId?.Any() == true)
                    query = query.Where(x => so.EventId.Contains(x.EventId));
                if (so?.SpeakerId?.Any() == true)
                    query = query.Where(x => x.SessionSpeakers!.Any(ss => so.SpeakerId.Contains(ss.SpeakerId)));
                if (so?.MinStartTime != null)
                    query = query.Where(x => x.StartTime >= so.MinStartTime);
                if (so?.MaxStartTime != null)
                    query = query.Where(x => x.StartTime <= so.MaxStartTime);
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.StartTime));
            // One to-one (Event) + one collection (SessionSpeakers) — no split query needed (only one collection nav).
            e.Includes((query, _) => query
                .Include(x => x.Event!)
                .Include(x => x.SessionSpeakers!).ThenInclude(ss => ss.Speaker!));
            e.Related(x => x.SessionSpeakers);
            e.AddProcessor<SessionProcessor>();
        });
}
