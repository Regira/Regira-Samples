using EventPlanner.Api.Data;
using EventPlanner.Api.Entities.Registrations;
using Microsoft.EntityFrameworkCore;

namespace EventPlanner.Api.Entities.Events;

// Cross-entity aggregates: Sessions and Registrations are independent entities with a back-ref FK to
// Event (not owned via Related()), so their counts are read from the store after fetch.
public class EventProcessor(EventPlannerDbContext dbContext) : Regira.Entities.Processing.Abstractions.IEntityProcessor<Event, EventIncludes>
{
    public async Task Process(IList<Event> items, EventIncludes? includes, CancellationToken token = default)
    {
        var eventIds = items.Select(x => x.Id).ToList();

        var sessionCounts = await dbContext.Sessions
            .Where(s => eventIds.Contains(s.EventId))
            .GroupBy(s => s.EventId)
            .Select(g => new { EventId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EventId, x => x.Count, token);

        var registrationCounts = await dbContext.Registrations
            .Where(r => eventIds.Contains(r.EventId) && r.Status != RegistrationStatus.Cancelled)
            .GroupBy(r => r.EventId)
            .Select(g => new { EventId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EventId, x => x.Count, token);

        foreach (var item in items)
        {
            item.SessionCount = sessionCounts.TryGetValue(item.Id, out var sc) ? sc : 0;
            item.RegistrationCount = registrationCounts.TryGetValue(item.Id, out var rc) ? rc : 0;
        }
    }
}
