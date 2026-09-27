using HelpDesk.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.Attachments;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;

namespace HelpDesk.Api.Entities.Tickets;

public static class TicketServiceConfiguration
{
    public static EntityServiceCollection<HelpDeskDbContext> AddTickets(this IEntityServiceCollection<HelpDeskDbContext> services)
        => services.For<Ticket, TicketSearchObject, TicketSortBy, TicketIncludes>(e =>
        {
            e.AddFilter<TicketQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                TicketSortBy.Oldest => query.OrderOrThenBy(x => x.Created),
                TicketSortBy.Priority => query.OrderOrThenByDescending(x => x.Priority!.Level).ThenBy(x => x.DueDate),
                TicketSortBy.DueDate => query.OrderOrThenBy(x => x.DueDate == null).ThenBy(x => x.DueDate),
                TicketSortBy.LastActivity => query.OrderOrThenByDescending(x => x.LastModified ?? x.Created),
                TicketSortBy.Code => query.OrderOrThenBy(x => x.Code),
                _ => query.OrderOrThenByDescending(x => x.Created)
            });
            // one Includes registration: to-ones every row shows load unconditionally, collections are flag-gated
            e.Includes((query, includes) =>
            {
                query = query
                    .Include(x => x.Customer)
                    .Include(x => x.AssignedEmployee)
                    .Include(x => x.SupportTeam)
                    .Include(x => x.Priority)
                    .Include(x => x.Status);
                if (includes?.HasFlag(TicketIncludes.Categories) == true)
                    query = query.Include(x => x.Categories!).ThenInclude(c => c.Category);
                if (includes?.HasFlag(TicketIncludes.Comments) == true)
                    query = query.Include(x => x.Comments!.OrderBy(c => c.Created)).ThenInclude(c => c.Author);
                if (includes?.HasFlag(TicketIncludes.Attachments) == true)
                    query = query.Include(x => x.Attachments!.OrderBy(a => a.SortOrder)).ThenInclude(a => a.Attachment);
                return query.AsSplitQuery();
            });
            e.Related(x => x.Categories);
            e.AddPrepper<TicketPrepper>();
            e.AddProcessor<TicketProcessor>();
            e.AddNormalizer<TicketNormalizer>();
            e.HasAttachments<HelpDeskDbContext, Ticket, TicketAttachment>(x => x.Attachments);
        });
}
