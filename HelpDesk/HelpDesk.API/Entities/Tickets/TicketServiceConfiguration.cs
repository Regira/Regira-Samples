using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.Attachments;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.EFcore.Extensions;
using HelpDesk.API.Data;
using HelpDesk.API.Entities.Statuses;

namespace HelpDesk.API.Entities.Tickets;

public static class TicketServiceConfiguration
{
    // 1/2 complex - primary entity of the app
    public static EntityServiceCollection<AppDbContext> AddTickets(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Ticket, TicketSearchObject, TicketSortBy, TicketIncludes>(e =>
        {
            e.AddFilter<TicketQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                TicketSortBy.Created => query.OrderOrThenBy(x => x.Created),
                TicketSortBy.CreatedDesc => query.OrderOrThenByDescending(x => x.Created),
                TicketSortBy.Priority => query.OrderOrThenBy(x => x.Priority!.Level),
                TicketSortBy.PriorityDesc => query.OrderOrThenByDescending(x => x.Priority!.Level),
                TicketSortBy.Title => query.OrderOrThenBy(x => x.Title),
                TicketSortBy.TitleDesc => query.OrderOrThenByDescending(x => x.Title),
                TicketSortBy.Status => query.OrderOrThenBy(x => x.Status!.SortOrder),
                _ => query.OrderOrThenByDescending(x => x.Created)
            });

            // Customer/AssignedEmployee/Priority/Status/SupportTeam: cheap to-one refs every list row
            // renders (customer name, assignee, priority/status badges) - always loaded, unconditionally.
            // Categories/Comments/Attachments: collections, flag-gated behind TicketIncludes.
            e.Includes((query, includes) =>
            {
                query = query
                    .Include(x => x.Customer!)
                    .Include(x => x.AssignedEmployee!)
                    .Include(x => x.Priority!)
                    .Include(x => x.Status!)
                    .Include(x => x.SupportTeam!);
                if (includes?.HasFlag(TicketIncludes.Categories) == true)
                    query = query.Include(x => x.Categories!).ThenInclude(tc => tc.Category);
                if (includes?.HasFlag(TicketIncludes.Comments) == true)
                    query = query.Include(x => x.Comments!.OrderBy(c => c.Created)).ThenInclude(c => c.Author);
                if (includes?.HasFlag(TicketIncludes.Attachments) == true)
                    query = query.Include(x => x.Attachments!).ThenInclude(a => a.Attachment);
                return query.AsSplitQuery();
            });

            e.Related(x => x.Categories);
            e.HasAttachments<AppDbContext, Ticket, TicketAttachment>(x => x.Attachments);

            // ClosedAt is server-owned: stamp it the moment the ticket lands on a "closed" status, clear
            // it on reopen, and preserve the original close time while it stays closed across edits.
            e.Prepare(async (ticket, dbContext) =>
            {
                var isClosed = await dbContext.Set<Status>().AsNoTracking()
                    .Where(s => s.Id == ticket.StatusId)
                    .Select(s => (bool?)s.IsClosed)
                    .FirstOrDefaultAsync() == true;

                if (!isClosed)
                {
                    ticket.ClosedAt = null;
                    return;
                }

                if (ticket.Id > 0)
                {
                    ticket.ClosedAt = await dbContext.Tickets.AsNoTracking()
                        .Where(t => t.Id == ticket.Id)
                        .Select(t => t.ClosedAt)
                        .FirstOrDefaultAsync() ?? DateTime.UtcNow;
                }
                else
                {
                    // Preserve a pre-set ClosedAt from seeding (back-dated closes); default to "now"
                    // for a freshly-created ticket that lands on a closed status via the normal API.
                    ticket.ClosedAt ??= DateTime.UtcNow;
                }
            });
        });
}
