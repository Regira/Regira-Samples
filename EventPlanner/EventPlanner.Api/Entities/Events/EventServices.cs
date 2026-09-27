using EventPlanner.Api.Data;
using EventPlanner.Api.Entities.Registrations;
using EventPlanner.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Preppers.Abstractions;
using Regira.Entities.Processing.Abstractions;
using Regira.Entities.QueryBuilders.Abstractions;

namespace EventPlanner.Api.Entities.Events;

public class EventQueryBuilder : FilteredQueryBuilderBase<Event, int, EventSearchObject>
{
    public override IQueryable<Event> Build(IQueryable<Event> query, EventSearchObject? so)
    {
        if (so == null) return query;
        if (so.CategoryId?.Any() == true) query = query.Where(x => so.CategoryId.Contains(x.CategoryId));
        if (so.LocationId?.Any() == true) query = query.Where(x => so.LocationId.Contains(x.LocationId));
        if (so.SpeakerId?.Any() == true)
            query = query.Where(x => x.Sessions!.Any(s => s.Speakers!.Any(sp => so.SpeakerId.Contains(sp.SpeakerId))));
        if (so.Status?.Any() == true) query = query.Where(x => so.Status.Contains(x.Status));
        if (so.IsFeatured.HasValue) query = query.Where(x => x.IsFeatured == so.IsFeatured);
        if (so.MinDate.HasValue) query = query.Where(x => x.EndDate >= so.MinDate);
        if (so.MaxDate.HasValue) query = query.Where(x => x.StartDate <= so.MaxDate);
        if (so.Upcoming.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            query = so.Upcoming.Value ? query.Where(x => x.EndDate >= today) : query.Where(x => x.EndDate < today);
        }
        return query;
    }
}

/// <summary>Row scope: employees never see draft events; administrators (and the seeder) see everything; anonymous callers nothing.</summary>
public class EventVisibilityQueryBuilder(ICurrentUser currentUser) : GlobalFilteredQueryBuilderBase<Event>
{
    public override IQueryable<Event> Build(IQueryable<Event> query, Regira.Entities.Models.Abstractions.ISearchObject<int>? so)
    {
        if (currentUser.CanManage) return query;
        if (!currentUser.IsAuthenticated) return query.Where(_ => false);
        return query.Where(x => x.Status != EventStatus.Draft);
    }
}

public class EventPrepper : EntityPrepperBase<Event>
{
    public override Task Prepare(Event modified, Event? original, CancellationToken token = default)
    {
        var errors = new Dictionary<string, string>();
        if (modified.EndDate < modified.StartDate)
            errors[nameof(Event.EndDate)] = "The end date cannot be before the start date.";

        if (modified.Sessions != null)
        {
            // sessions may start the evening before (UTC vs local) or end just after midnight: allow one day of slack
            var min = modified.StartDate.AddDays(-1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var max = modified.EndDate.AddDays(2).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var i = 0;
            foreach (var session in modified.Sessions)
            {
                if (session.EndTime <= session.StartTime)
                    errors[$"Sessions[{i}].EndTime"] = $"Session '{session.Title}' must end after it starts.";
                else if (session.StartTime < min || session.EndTime > max)
                    errors[$"Sessions[{i}].StartTime"] = $"Session '{session.Title}' must take place during the event.";
                // a speaker listed twice in one session would violate the unique index: dedupe silently
                if (session.Speakers != null)
                    session.Speakers = session.Speakers.GroupBy(s => s.SpeakerId).Select(g => g.OrderByDescending(s => s.Id).First()).ToList();
                i++;
            }
        }

        if (errors.Count > 0)
        {
            var ex = new EntityInputException<Event>("Saving event failed");
            foreach (var (key, value) in errors) ex.InputErrors[key] = value;
            throw ex;
        }
        return Task.CompletedTask;
    }
}

/// <summary>Fills the [NotMapped] registration counters (and the caller's own registration id).</summary>
public class EventProcessor(EventPlannerDbContext dbContext, ICurrentUser currentUser) : IEntityProcessor<Event, EventIncludes>
{
    public async Task Process(IList<Event> items, EventIncludes? includes, CancellationToken token = default)
    {
        if (items.Count == 0) return;
        var ids = items.Select(x => x.Id).ToList();

        var counts = await dbContext.Registrations.AsNoTracking()
            .Where(r => ids.Contains(r.EventId))
            .GroupBy(r => new { r.EventId, r.Status })
            .Select(g => new { g.Key.EventId, g.Key.Status, Count = g.Count() })
            .ToListAsync(token);

        var mine = new Dictionary<int, int>();
        if (currentUser.UserId is { } userId)
        {
            mine = await dbContext.Registrations.AsNoTracking()
                .Where(r => r.UserId == userId && ids.Contains(r.EventId))
                .ToDictionaryAsync(r => r.EventId, r => r.Id, token);
        }

        var sessionIds = items.Where(x => x.Sessions != null).SelectMany(x => x.Sessions!).Select(s => s.Id).ToList();
        var sessionCounts = new Dictionary<int, int>();
        if (sessionIds.Count > 0)
        {
            sessionCounts = await dbContext.RegistrationSessions.AsNoTracking()
                .Where(rs => sessionIds.Contains(rs.SessionId) && rs.Registration!.Status != RegistrationStatus.Cancelled)
                .GroupBy(rs => rs.SessionId)
                .Select(g => new { SessionId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.SessionId, x => x.Count, token);
        }

        foreach (var item in items)
        {
            item.RegistrationCount = counts.Where(c => c.EventId == item.Id && c.Status == RegistrationStatus.Confirmed).Sum(c => c.Count);
            item.WaitlistCount = counts.Where(c => c.EventId == item.Id && c.Status == RegistrationStatus.Waitlisted).Sum(c => c.Count);
            item.MyRegistrationId = mine.TryGetValue(item.Id, out var regId) ? regId : null;
            if (item.Sessions != null)
                foreach (var session in item.Sessions)
                    session.RegisteredCount = sessionCounts.GetValueOrDefault(session.Id);
        }
    }
}
