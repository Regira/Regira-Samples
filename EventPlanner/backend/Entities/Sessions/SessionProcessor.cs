using EventPlanner.Api.Data;
using EventPlanner.Api.Entities.Registrations;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Processing.Abstractions;

namespace EventPlanner.Api.Entities.Sessions;

// Cross-entity aggregate: RegistrationSession rows are owned by Registration, not by Session, so
// SeatsTaken is read from the store after fetch rather than diffed from an incoming collection.
public class SessionProcessor(EventPlannerDbContext dbContext) : IEntityProcessor<Session, EntityIncludes>
{
    public async Task Process(IList<Session> items, EntityIncludes? includes, CancellationToken token = default)
    {
        var sessionIds = items.Select(x => x.Id).ToList();
        var counts = await dbContext.RegistrationSessions
            .Where(rs => sessionIds.Contains(rs.SessionId) && rs.Registration!.Status != RegistrationStatus.Cancelled)
            .GroupBy(rs => rs.SessionId)
            .Select(g => new { SessionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SessionId, x => x.Count, token);

        foreach (var item in items)
            item.SeatsTaken = counts.TryGetValue(item.Id, out var count) ? count : 0;
    }
}
