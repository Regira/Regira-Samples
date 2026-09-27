using HelpDesk.Api.Data;
using HelpDesk.Api.Entities.Persons;
using HelpDesk.Api.Infrastructure.Security;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.QueryBuilders.Abstractions;

namespace HelpDesk.Api.Entities.Tickets;

/// <summary>
/// Row security for tickets: staff (and the system) see every ticket, a customer only the tickets they reported,
/// an anonymous or unknown caller nothing. Global, so it also scopes Details(id) and the write existence checks.
/// </summary>
public class TicketAccessFilter(CurrentUser currentUser) : GlobalFilteredQueryBuilderBase<Ticket>
{
    public override IQueryable<Ticket> Build(IQueryable<Ticket> query, ISearchObject<int>? so)
    {
        if (currentUser.IsStaff) return query;
        var userId = currentUser.UserId;
        if (userId == null) return query.Where(_ => false);
        return query.Where(t => t.Customer!.UserId == userId);
    }
}

/// <summary>
/// Attachment links are a separately registered entity: without this filter a customer could list or download
/// another customer's files by id.
/// </summary>
public class TicketAttachmentAccessFilter(CurrentUser currentUser, HelpDeskDbContext dbContext) : GlobalFilteredQueryBuilderBase<TicketAttachment>
{
    public override IQueryable<TicketAttachment> Build(IQueryable<TicketAttachment> query, ISearchObject<int>? so)
    {
        if (currentUser.IsStaff) return query;
        var userId = currentUser.UserId;
        if (userId == null) return query.Where(_ => false);
        return query.Where(a => dbContext.Tickets.Any(t => t.Id == a.ObjectId && t.Customer!.UserId == userId));
    }
}

/// <summary>Customers see themselves and the employees (names on comments and assignments), never other customers.</summary>
public class PersonAccessFilter(CurrentUser currentUser) : GlobalFilteredQueryBuilderBase<Person>
{
    public override IQueryable<Person> Build(IQueryable<Person> query, ISearchObject<int>? so)
    {
        if (currentUser.IsStaff) return query;
        var userId = currentUser.UserId;
        if (userId == null) return query.Where(_ => false);
        return query.Where(p => p.Role == PersonRole.Employee || p.UserId == userId);
    }
}
