using HelpDesk.Api.Data;
using HelpDesk.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Controllers;

/// <summary>
/// Support-center KPIs. Cross-entity aggregates bypass the entity pipeline (and its row filter), so the endpoint is
/// staff-only rather than re-implementing customer scoping.
/// </summary>
[ApiController, Route("dashboard"), Authorize(Roles = Roles.Staff)]
public class DashboardController(HelpDeskDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken token)
    {
        var now = DateTime.UtcNow;
        var since = now.Date.AddDays(-29);
        var tickets = db.Tickets.AsNoTracking();

        var open = await tickets.CountAsync(t => !t.Status!.IsClosed, token);
        var unassigned = await tickets.CountAsync(t => !t.Status!.IsClosed && t.AssignedEmployeeId == null, token);
        var overdue = await tickets.CountAsync(t => !t.Status!.IsClosed && t.DueDate != null && t.DueDate < now, token);
        var closedLast30 = await tickets.CountAsync(t => t.ClosedAt != null && t.ClosedAt >= since, token);

        var byStatus = await db.Statuses.AsNoTracking()
            .OrderBy(s => s.SortOrder)
            .Select(s => new { s.Id, s.Title, s.Color, s.IsClosed, Count = db.Tickets.Count(t => t.StatusId == s.Id) })
            .ToListAsync(token);
        var byPriority = await db.Priorities.AsNoTracking()
            .OrderByDescending(p => p.Level)
            .Select(p => new { p.Id, p.Title, p.Color, Count = db.Tickets.Count(t => t.PriorityId == p.Id && !t.Status!.IsClosed) })
            .ToListAsync(token);
        var byTeam = await db.SupportTeams.AsNoTracking()
            .OrderBy(x => x.Title)
            .Select(x => new
            {
                x.Id, x.Title, x.Color,
                Open = db.Tickets.Count(t => t.SupportTeamId == x.Id && !t.Status!.IsClosed),
                Overdue = db.Tickets.Count(t => t.SupportTeamId == x.Id && !t.Status!.IsClosed && t.DueDate != null && t.DueDate < now)
            })
            .ToListAsync(token);

        // per-day series: project flat, shape after ToListAsync
        var createdDays = await tickets.Where(t => t.Created >= since).Select(t => t.Created.Date).ToListAsync(token);
        var closedDays = await tickets.Where(t => t.ClosedAt != null && t.ClosedAt >= since).Select(t => t.ClosedAt!.Value.Date).ToListAsync(token);
        var series = Enumerable.Range(0, 30).Select(i => since.AddDays(i)).Select(day => new
        {
            Date = day.ToString("yyyy-MM-dd"),
            Created = createdDays.Count(d => d == day),
            Closed = closedDays.Count(d => d == day)
        });

        // average first response (hours) over the last 30 days: two columns, averaged in memory
        var responses = await tickets.Where(t => t.FirstResponseAt != null && t.Created >= since)
            .Select(t => new { t.Created, t.FirstResponseAt }).ToListAsync(token);
        var avgFirstResponseHours = responses.Count == 0 ? (double?)null
            : Math.Round(responses.Average(r => (r.FirstResponseAt!.Value - r.Created).TotalHours), 1);

        return Ok(new
        {
            item = new
            {
                total = await tickets.CountAsync(token),
                open, unassigned, overdue, closedLast30, avgFirstResponseHours,
                byStatus, byPriority, byTeam, series
            }
        });
    }
}
