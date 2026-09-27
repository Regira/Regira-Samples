using EventPlanner.Api.Data;
using EventPlanner.Api.Entities.Events;
using EventPlanner.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Preppers.Abstractions;
using Regira.Entities.QueryBuilders.Abstractions;

namespace EventPlanner.Api.Entities.Registrations;

public class RegistrationQueryBuilder : FilteredQueryBuilderBase<Registration, int, RegistrationSearchObject>
{
    public override IQueryable<Registration> Build(IQueryable<Registration> query, RegistrationSearchObject? so)
    {
        if (so == null) return query;
        if (so.EventId?.Any() == true) query = query.Where(x => so.EventId.Contains(x.EventId));
        if (so.UserId?.Any() == true) query = query.Where(x => so.UserId.Contains(x.UserId!));
        if (so.Status?.Any() == true) query = query.Where(x => so.Status.Contains(x.Status));
        if (so.SessionId?.Any() == true) query = query.Where(x => x.Sessions!.Any(s => so.SessionId.Contains(s.SessionId)));
        if (so.Upcoming.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            query = so.Upcoming.Value ? query.Where(x => x.Event!.EndDate >= today) : query.Where(x => x.Event!.EndDate < today);
        }
        // Registration has no NormalizedContent, so the global ?q= filter does not apply: match employee or event here
        foreach (var term in (so.Q ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim('*')).Where(t => t.Length > 0))
        {
            var pattern = $"%{term}%";
            query = query.Where(x =>
                EF.Functions.Like(x.User!.FirstName!, pattern) || EF.Functions.Like(x.User!.LastName!, pattern)
                || EF.Functions.Like(x.User!.Email!, pattern) || EF.Functions.Like(x.Event!.Title!, pattern));
        }
        return query;
    }
}

/// <summary>Row scope: employees only ever see (and therefore can only modify/delete) their own registrations.</summary>
public class RegistrationScopeQueryBuilder(ICurrentUser currentUser) : GlobalFilteredQueryBuilderBase<Registration>
{
    public override IQueryable<Registration> Build(IQueryable<Registration> query, ISearchObject<int>? so)
    {
        if (currentUser.CanManage) return query;
        var userId = currentUser.UserId;
        if (userId == null) return query.Where(_ => false);
        return query.Where(x => x.UserId == userId);
    }
}

/// <summary>
/// Business rules of a registration: who it is for, whether the event is open, event capacity (wait list),
/// session selection (must belong to the event, capacity per session).
/// Registered before the Related() sync, so it sees the incoming session rows unsynced.
/// </summary>
public class RegistrationPrepper(EventPlannerDbContext dbContext, ICurrentUser currentUser) : EntityPrepperBase<Registration>
{
    public override async Task Prepare(Registration modified, Registration? original, CancellationToken token = default)
    {
        var isCreate = original == null;
        var errors = new Dictionary<string, string>();

        // ---- who / which event (immutable after create: [ServerOwned] restores them on update) ----
        if (isCreate)
        {
            if (!currentUser.CanManage)
                modified.UserId = currentUser.UserId ?? throw Fail(nameof(Registration.UserId), "You must be signed in to register.");
            else if (string.IsNullOrWhiteSpace(modified.UserId))
                throw Fail(nameof(Registration.UserId), "Select the employee to register.");
            else if (!await dbContext.Users.AnyAsync(u => u.Id == modified.UserId, token))
                throw Fail(nameof(Registration.UserId), "Unknown employee.");
        }
        else
        {
            modified.UserId = original!.UserId;
            modified.EventId = original.EventId;
        }

        var ev = await dbContext.Events.AsNoTracking()
            .Where(x => x.Id == modified.EventId)
            .Select(x => new { x.Id, x.Status, x.EndDate, x.MaxParticipants })
            .FirstOrDefaultAsync(token)
            ?? throw Fail(nameof(Registration.EventId), "Select an existing event.");

        if (isCreate)
        {
            if (!currentUser.IsSystem)
            {
                if (ev.Status != EventStatus.Published && !currentUser.IsAdmin)
                    throw Fail(nameof(Registration.EventId), "This event is not open for registration.");
                if (ev.EndDate < DateOnly.FromDateTime(DateTime.UtcNow) && !currentUser.IsAdmin)
                    throw Fail(nameof(Registration.EventId), "This event has already ended.");
            }
            var duplicate = await dbContext.Registrations.AnyAsync(r => r.EventId == ev.Id && r.UserId == modified.UserId, token)
                || PendingRegistrations(modified).Any(r => r.EventId == ev.Id && r.UserId == modified.UserId);
            if (duplicate)
                throw Fail(nameof(Registration.EventId), "This employee is already registered for this event.");
        }

        // ---- status (employees cannot promote themselves off the wait list) ----
        if (!currentUser.CanManage)
        {
            if (modified.Status == RegistrationStatus.Cancelled)
            {
                // cancelling is always allowed
            }
            else if (isCreate || original!.Status == RegistrationStatus.Cancelled)
            {
                modified.Status = await IsEventFull(ev.Id, ev.MaxParticipants, modified, token)
                    ? RegistrationStatus.Waitlisted
                    : RegistrationStatus.Confirmed;
            }
            else
            {
                modified.Status = original.Status;
            }
        }

        // ---- session selection ----
        if (modified.Sessions != null)
        {
            modified.Sessions = modified.Sessions.GroupBy(s => s.SessionId).Select(g => g.OrderByDescending(s => s.Id).First()).ToList();
            var requested = modified.Sessions.Select(s => s.SessionId).ToList();
            var sessions = await dbContext.Sessions.AsNoTracking()
                .Where(s => requested.Contains(s.Id))
                .Select(s => new { s.Id, s.EventId, s.Title, s.Capacity })
                .ToListAsync(token);

            foreach (var id in requested.Where(id => sessions.All(s => s.Id != id || s.EventId != ev.Id)))
                errors[nameof(Registration.Sessions)] = $"Session #{id} does not belong to this event.";

            // capacity: only newly picked sessions of an active registration are checked (never block keeping a seat)
            if (errors.Count == 0 && modified.Status != RegistrationStatus.Cancelled && !currentUser.IsSystem)
            {
                var previouslyPicked = original?.Status == RegistrationStatus.Cancelled
                    ? []
                    : original?.Sessions?.Select(s => s.SessionId).ToHashSet() ?? [];
                var added = sessions.Where(s => !previouslyPicked.Contains(s.Id)).ToList();
                if (added.Count > 0)
                {
                    var addedIds = added.Select(s => s.Id).ToList();
                    var taken = await dbContext.RegistrationSessions.AsNoTracking()
                        .Where(rs => addedIds.Contains(rs.SessionId) && rs.RegistrationId != modified.Id
                                     && rs.Registration!.Status != RegistrationStatus.Cancelled)
                        .GroupBy(rs => rs.SessionId)
                        .Select(g => new { g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.Key, x => x.Count, token);
                    foreach (var session in added)
                    {
                        var pending = PendingRegistrations(modified)
                            .Count(r => r.Status != RegistrationStatus.Cancelled && r.Sessions?.Any(s => s.SessionId == session.Id) == true);
                        if (taken.GetValueOrDefault(session.Id) + pending >= session.Capacity)
                            errors[nameof(Registration.Sessions)] = $"Session '{session.Title}' is full.";
                    }
                }
            }
        }

        if (errors.Count > 0)
        {
            var ex = new EntityInputException<Registration>("Saving registration failed");
            foreach (var (key, value) in errors) ex.InputErrors[key] = value;
            throw ex;
        }
    }

    private async Task<bool> IsEventFull(int eventId, int? max, Registration self, CancellationToken token)
    {
        if (max == null) return false;
        var confirmed = await dbContext.Registrations.AsNoTracking()
            .CountAsync(r => r.EventId == eventId && r.Status == RegistrationStatus.Confirmed && r.Id != self.Id, token);
        confirmed += PendingRegistrations(self).Count(r => r.EventId == eventId && r.Status == RegistrationStatus.Confirmed);
        return confirmed >= max;
    }

    /// <summary>Registrations queued in the same SaveChanges batch (bulk writes/seeding) — invisible to a DB query.</summary>
    private IEnumerable<Registration> PendingRegistrations(Registration self)
        => dbContext.ChangeTracker.Entries<Registration>()
            .Where(e => e.State == EntityState.Added && !ReferenceEquals(e.Entity, self))
            .Select(e => e.Entity);

    private static EntityInputException<Registration> Fail(string field, string message)
    {
        var ex = new EntityInputException<Registration>(message);
        ex.InputErrors[field] = message;
        return ex;
    }
}
